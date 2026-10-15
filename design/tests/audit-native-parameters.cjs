const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');
const standaloneFixture = require('./standalone-fixture.cjs');

// 以原生 BuildProperties 为契约，核对参数标识与编辑器类型。
(async () => {
  const root = path.resolve(__dirname, '../..');
  const design = path.join(root, 'design');
  const catalog = JSON.parse(fs.readFileSync(path.join(design, 'catalog.json'), 'utf8')).flatMap(group => group.items);
  const native = file => fs.readFileSync(path.join(root, 'demo/CodeWF.Toolkit.Demo.Avalonia', file), 'utf8');
  for (const type of ['Helper', 'Transport', 'DataTable']) {
    assert.match(native(`ViewModels/Pages/${type}DemoViewModel.cs`), /Id = nameof\(Input\)[\s\S]*?Kind = PropertyItemKind.MultilineText/);
  }
  assert.match(native('ViewModels/Pages/FileSystemDemoViewModel.cs'), /Id = nameof\(TransferSize\)[\s\S]*?Kind = PropertyItemKind.Number/);
  const explicit = {
    'image-icon':['ic.sizes','ic.compress','ic.quality','ic.source','ic.dest'],
    'image-qr':['qr.size','qr.ecc','qr.charset','img.quality','img.preview'],
    bus:['bus.name','bus.async','bus.scan','bus.order'],
    'log-core':['lg.level','lg.console','lg.rotate','lg.maxmb','lg.days','lg.folder','lg.feed'],
    'log-microsoft':['mel.level','mel.scope','mel.user','mel.queue','mel.provider','mel.feed'],
    'net-packet':['pkt.systemid','pkt.version','pkt.bigendian','pkt.offset'],
    'scene-format':['files.format','files.retry','files.bom'],
    'scene-archive':['sz.algorithm','sz.utf8','sz.samples','cfg.path','cfg.bom'],
    'bus-integrations':['busint.mode','busint.port','busint.heartbeat','busint.resend']
  };
  const types = {'image-icon':'ImageIcon','image-qr':'ImageQr',bus:'EventBus','log-core':'LoggingCore','log-microsoft':'LoggingMicrosoft','net-packet':'NetPacket','scene-format':'FilesFormat','scene-archive':'FilesArchive','bus-integrations':'EventBusIntegrations'};
  for (const [route, ids] of Object.entries(explicit)) {
    const source = native(`Views/Pages/${types[route]}Page.cs`);
    const actual = [...source.matchAll(/DemoProperty\.(?:ReadOnly|Bool|Number|Choice)\("([^"]+)"/g)].map(match => match[1]);
    assert.deepEqual(new Set(ids), new Set(actual), `Native source property IDs: ${route}`);
  }
  const expected = entry => /^HelperDemoPage|^TransportDemoPage|^DataTableDemoPage/.test(entry.page) ? ['Input']
    : entry.page.startsWith('FileSystemDemoPage') ? ['TransferSize']
    : explicit[entry.prototypeId] || (entry.prototypeId === 'flow-runtime' ? explicit.bus : entry.prototypeId === 'flow-lab' ? explicit['scene-format'] : []);
  const html = fs.readdirSync(design).find(name => /^CodeWF\..*Demo原型\.html$/.test(name));
  const fixture = standaloneFixture(path.join(design, html));
  const browser = await chromium.launch({ headless:true });
  const output = process.env.CODEWF_AUDIT_OUTPUT || path.join(__dirname, 'artifacts');
  fs.mkdirSync(output, {recursive:true});
  const evidence = [];
  try {
    const page = await browser.newPage({ viewport:{width:1440,height:900} });
    const errors=[]; page.on('pageerror', error => errors.push(error.message));
    await fixture.attach(page); await page.goto(fixture.url);
    for (const culture of ['zh-CN','en-US']) {
      await page.evaluate(culture => setLang(culture), culture);
      for (const entry of catalog) {
        const route=entry.prototypeId;
        await page.evaluate(route => openDoc(route), route);
        if (route.startsWith('flow-')) await page.locator('[data-native-step="0"]').click();
        const contract=await page.evaluate(route => (PROTO[route].props||[]).map(p=>({id:p.id,kind:p.kind,type:p.type,value:p.value,label:p.label})),route);
        if (route==='welcome') continue;
        assert.deepEqual(contract.map(p=>p.id),expected(entry),`${culture} property IDs: ${route}`);
        assert.equal(await page.locator('#pgCnt').innerText(),`(${contract.length})`,`${culture} count: ${route}`);
        assert.equal(await page.locator('#pGrid .rw').count(),contract.length,`${culture} rows: ${route}`);
        assert.ok(contract.every(p=>!/^\w+\.\w+$/.test(p.label||'')),`${culture} translated native labels: ${route}`);
        if (expected(entry)[0]==='Input') {
          assert.equal(contract[0].type,'multiline');
          const input=page.locator(`#seg-${route} .catalog-input`).first();
          const editor=page.locator('#pGrid [data-id="Input"]');
          assert.equal(await input.inputValue(),String(contract[0].value));
          await editor.fill(`property-${route}`); await editor.dispatchEvent('change');
          assert.equal(await input.inputValue(),`property-${route}`,`property to page: ${route}`);
          await input.fill(`page-${route}`);
          assert.equal(await editor.inputValue(),`page-${route}`,`page to property: ${route}`);
          await input.fill(String(contract[0].value));
        }
        evidence.push({culture,route,properties:contract.map(p=>p.id)});
      }
      for (const [scene,routes] of [['flow-runtime',['bus','bus-integrations','bus','bus-integrations','log-core']],['flow-lab',['scene-format','scene-archive','image-icon','image-qr']]]) {
        await page.evaluate(scene=>openDoc(scene),scene);
        for(let index=0;index<routes.length;index++) {
          await page.locator(`[data-native-step="${index}"]`).click();
          assert.deepEqual(await page.evaluate(scene=>PROTO[scene].props.map(p=>p.id),scene),explicit[routes[index]],`${culture} scene step ${scene}/${index}`);
          assert.equal(await page.locator('#pGrid .rw').count(),explicit[routes[index]].length);
        }
      }
      await page.evaluate(()=>openDoc('stringextensions'));
      await page.locator('#pGrid [data-id="Input"]').fill('one,,two');
      await page.locator('#pGrid [data-id="Input"]').dispatchEvent('change');
      await page.locator('.catalog-run').click();
      assert.match(await page.locator('#seg-stringextensions .catalog-result').innerText(),/one \/ two/);
      assert.match(await page.locator('#seg-stringextensions .catalog-result').innerText(),culture==='en-US'?/preview/:/示意/);
      await page.screenshot({path:path.join(output,`parameters-${culture}.png`)});
      await page.evaluate(()=>openDoc('tcpsocketclientfilesystemfeature'));
      const size=page.locator('#pGrid [data-id="TransferSize"]');
      await size.fill('5242880'); await size.dispatchEvent('change');
      assert.equal(await page.locator('#seg-tcpsocketclientfilesystemfeature').getAttribute('data-transfer-size'),'5242880');
      await page.locator('[data-op="start"]').click(); await page.locator('[data-op="upload"]').click();
      await page.waitForFunction(()=>document.querySelector('#seg-tcpsocketclientfilesystemfeature .catalog-result').textContent.includes('Bytes = 5242880'));
      await page.locator('[data-op="stop"]').click();
    }
    await fixture.verify(page); assert.deepEqual(errors,[]);
    fs.writeFileSync(path.join(output,'native-parameters.json'),JSON.stringify(evidence,null,2));
    console.log(`Passed: native property IDs/types, ${catalog.length} routes × 2 languages, bidirectional Input, TransferSize effect, 18 scene steps, standalone, browser errors=0.`);
  } finally { await browser.close(); fixture.dispose(); }
})().catch(error=>{console.error(error);process.exitCode=1;});
