/* 参数契约与原生 BuildProperties 保持一致，浏览器结果仍为交互预览。 */
"use strict";
(function () {
  var entries = window.CodeWFDemoCatalog.flatMap(function (group) { return group.items; });
  var hosts = {}, sceneSteps = {};
  function text(key) { return window.CodeWFCatalogText[LANG][key] || key; }
  function label(path) {
    if (path === 'Input') return text('Input');
    var parts = path.split('.');
    return window.CodeWFPropertyText[LANG]?.[parts[0]]?.[parts[1]] || path;
  }
  function prop(id, name, kind, value, options, minimum, maximum) {
    var item = { id:id, resource:name, kind:kind, value:value, options:options, min:minimum, max:maximum };
    if (kind === 'text' || kind === 'multiline') item.type = kind;
    item.apply = function (next) {
      if (kind === 'num') item.value = Math.max(minimum ?? -Infinity, Math.min(maximum ?? Infinity, Number(next)));
      var host = hosts[id];
      if (!host?.isConnected) return;
      var target = id === 'Input' ? host.querySelector('.catalog-input') : host.querySelector('[data-native-property="' + id + '"]');
      if (target) { if (target.type === 'checkbox') target.checked = Boolean(item.value); else target.value = item.value; }
      if (id === 'TransferSize') host.dataset.transferSize = String(item.value);
      if (id === 'img.preview') { var canvas = host.querySelector('canvas'); if (canvas) canvas.hidden = !item.value; }
      var state = host.querySelector('.native-parameter-state');
      if (state) state.textContent = text('Preview') + '\n' + (PROTO[host.dataset.nativePage]?.props || []).map(function (p) { return label(p.resource) + ' = ' + p.value; }).join('\n');
    };
    return item;
  }
  function set(id, properties) { if (PROTO[id]) PROTO[id].props = properties; }
  entries.forEach(function (entry) {
    if (/^(HelperDemoPage|TransportDemoPage|DataTableDemoPage)/.test(entry.page)) {
      set(entry.prototypeId, [prop('Input', 'Input', 'multiline', entry.page.startsWith('TransportDemoPage') ? 'Hello CodeWF' : (entry.input || ''))]);
    } else if (entry.page.startsWith('FileSystemDemoPage')) {
      set(entry.prototypeId, [prop('TransferSize', 'TransferSize', 'num', 4194304, null, 1, 16777216)]);
    }
  });
  set('image-icon', [
    prop('ic.sizes','ImageIcon.IncludedSizes','ro','16,24,32,48,64,128,256'),
    prop('ic.compress','ImageIcon.PNGCompression','sw',true), prop('ic.quality','ImageIcon.Quality','num',92,null,1,100),
    prop('ic.source','ImageIcon.SourceImage','ro','Assets/logo.png'), prop('ic.dest','ImageIcon.OutputDirectory','ro','%TEMP%/CodeWFToolkitDemo/image')
  ]);
  set('image-qr', [
    prop('qr.size','ImageQr.QRCodeSize','ro',label('ImageQr._400400FinalCanvasWidth500')),
    prop('qr.ecc','ImageQr.ErrorCorrection','ro','H（ErrorCorrectionLevel.H）'),
    prop('qr.charset','ImageQr.CharacterSet','ro','UTF-8（DisableECI = true）'),
    prop('img.quality','ImageQr.OutputQuality','num',92,null,1,100), prop('img.preview','ImageQr.GeneratePreview','sw',true)
  ]);
  set('bus', [prop('bus.name','EventBus.BusInstance','ro','EventBus'), prop('bus.async','EventBus.PublishAsynchronously','sw',false),
    prop('bus.scan','EventBus.AssemblyScanning','sw',false), prop('bus.order','EventBus.HandlerOrder','num',1,null,0,100)]);
  set('log-core', [prop('lg.level','LoggingCore.MinimumLevel','sel','Information',['Trace','Debug','Information','Warning','Error']),
    prop('lg.console','LoggingCore.ConsoleOutput','sw',true), prop('lg.rotate','LoggingCore.FileRotation','sw',true),
    prop('lg.maxmb','LoggingCore.PerFileLimitMB','num',32,null,1,1024), prop('lg.days','LoggingCore.RetentionDays','num',30,null,1,365),
    prop('lg.folder','LoggingCore.LogDirectory','ro','%LOCALAPPDATA%/CodeWFToolkitDemo/logs'), prop('lg.feed','LoggingCore.EventCapacity','ro','300')]);
  set('log-microsoft', [prop('mel.level','LoggingMicrosoft.MinimumLevel','sel','Information',['Trace','Debug','Information','Warning','Error']),
    prop('mel.scope','LoggingMicrosoft.IncludeScopes','sw',true), prop('mel.user','LoggingMicrosoft.UserMessageMarker','sw',false),
    prop('mel.queue','LoggingMicrosoft.QueueOverflowPolicy','sel','DropOldest',['DropOldest','DropNewest','Block']),
    prop('mel.provider','LoggingMicrosoft.Provider','ro','CodeWFLoggerProvider'), prop('mel.feed','LoggingMicrosoft.EventFlow','ro',label('LoggingMicrosoft.SharedWithStaticLogger'))]);
  set('net-packet', [prop('pkt.systemid','systemId','num',1,null,0,255), prop('pkt.version','version','num',1,null,0,255),
    prop('pkt.bigendian','NetPacket.LittleEndianInt32','sw',true), prop('pkt.offset','NetPacket.BitFieldLayout','sw',false)]);
  set('scene-format', [prop('files.format','FilesFormat.DefaultFormat','sel','Yaml',['Json','Yaml','Csv','Excel']),
    prop('files.retry','FilesFormat.WriteRetries','num',3,null,0,10), prop('files.bom','UTF-8 BOM','sw',false)]);
  set('scene-archive', [prop('sz.algorithm','FilesArchive.CompressionAlgorithm','ro','LZMA（ZipWriterOptions）'),
    prop('sz.utf8','FilesArchive.UTF8EntryNames','sw',true), prop('sz.samples','FilesArchive.SampleFileCount','num',3,null,1,20),
    prop('cfg.path','FilesArchive.FileLocation','ro','%TEMP%/CodeWFToolkitDemo/archive/App.config'), prop('cfg.bom','UTF-8 BOM','sw',false)]);
  set('bus-integrations', [prop('busint.mode','EventBusIntegrations.HostIntegration','sel','Socket',[label('EventBusIntegrations.InProcess'),'Prism','ASP.NET Core','Socket']),
    prop('busint.port','EventBusIntegrations.Port','num',39561,null,1024,65535),
    prop('busint.heartbeat','EventBusIntegrations.HeartbeatSeconds','num',10,null,1,120), prop('busint.resend','EventBusIntegrations.ReplayOfflineMessages','sw',true)]);

  function scene(id, routes, captions) {
    var index = sceneSteps[id] || 0;
    PROTO[id].props = PROTO[routes[index]].props;
    PROTO[id].render = function (el) {
      var names = captions();
      el.innerHTML = '<div class="demo"><p>' + esc(text('Preview')) + '</p><div class="row">' + routes.map(function (route, i) {
        return '<button class="btn' + (i === index ? ' pri' : '') + '" data-native-step="' + i + '">' + (i + 1) + ' · ' + esc(names[i]) + '</button>';
      }).join('') + '</div></div><div class="native-scene-page"></div>';
      function show(next) {
        index = next; sceneSteps[id] = next; PROTO[id].props = PROTO[routes[next]].props;
        el.querySelectorAll('[data-native-step]').forEach(function (button) { button.classList.toggle('pri', Number(button.dataset.nativeStep) === next); });
        renderInto(routes[next], el.querySelector('.native-scene-page'));
        if (PROTO[routes[next]].after) PROTO[routes[next]].after();
        renderPGrid(id);
      }
      el.querySelectorAll('[data-native-step]').forEach(function (button) { button.onclick = function () { show(Number(button.dataset.nativeStep)); }; });
      show(index);
    };
  }
  scene('flow-runtime',['bus','bus-integrations','bus','bus-integrations','log-core'],function () { return text('RuntimeTourSteps').split('|'); });
  // 复用已有场景资源，按原生顺序组合四类页面。
  function labCaptions() { return LANG === 'en-US' ? ['Formats','Archive and configuration','Icons','QR and images'] : ['格式转换','归档与配置','图标','二维码与图像']; }
  scene('flow-lab',['scene-format','scene-archive','image-icon','image-qr'],labCaptions);
  var previousRender = renderInto;
  renderInto = function (id, el) {
    previousRender(id, el);
    if (!el || id === 'welcome' || id.startsWith('flow-')) return;
    el.dataset.nativePage = id;
    var properties = PROTO[id]?.props || [];
    if (id === 'image-qr') { var qrSize = el.querySelector('#qrSize'); if (qrSize) { qrSize.value = 400; qrSize.readOnly = true; } }
    var input = el.querySelector('.catalog-input');
    if (properties[0]?.id === 'Input' && input) {
      if (input.tagName === 'INPUT') { var multiline = document.createElement('textarea'); multiline.className = input.className; multiline.value = input.value; input.replaceWith(multiline); input = multiline; }
      input.value = properties[0].value;
      input.addEventListener('input', function () {
        properties[0].value = input.value;
        var editor = document.querySelector('#pGrid [data-id="Input"]'); if (editor) editor.value = input.value;
      });
    }
    var selectors = {'pkt.systemid':'#pktSys','pkt.version':'#pktVer','busint.port':'#esPort','busint.heartbeat':'#esHb'};
    properties.forEach(function (p) {
      hosts[p.id] = el;
      var target = selectors[p.id] && el.querySelector(selectors[p.id]);
      if (target) { target.dataset.nativeProperty = p.id; target.value = p.value; target.addEventListener('input',function () { p.value = Number(target.value); var editor = document.querySelector('#pGrid [data-id="'+p.id+'"]'); if (editor) editor.value = p.value; }); }
    });
    if (properties[0]?.id === 'TransferSize') el.dataset.transferSize = String(properties[0].value);
    if (properties.length && !properties.some(function (p) { return p.id === 'Input'; })) {
      var state = document.createElement('pre'); state.className = 'catalog-result native-parameter-state'; el.appendChild(state);
      properties[0].apply(properties[0].value);
    }
    if (!el.querySelector('.native-preview-note')) { var note=document.createElement('p'); note.className='native-preview-note'; note.textContent=text('Preview'); el.prepend(note); }
  };
  var previousGrid = renderPGrid;
  renderPGrid = function (id) {
    if (PROTO[id]?.props?.some(function (p) { return p.id === 'qr.size'; })) PROTO[id].props.find(function (p) { return p.id === 'qr.size'; }).value = label('ImageQr._400400FinalCanvasWidth500');
    if (PROTO[id]?.props?.some(function (p) { return p.id === 'mel.feed'; })) PROTO[id].props.find(function (p) { return p.id === 'mel.feed'; }).value = label('LoggingMicrosoft.SharedWithStaticLogger');
    (PROTO[id]?.props || []).forEach(function (p) { if (p.resource) p.label = label(p.resource); });
    previousGrid(id);
    (PROTO[id]?.props || []).forEach(function (p) {
      var editor = document.querySelector('#pGrid [data-id="' + p.id + '"]');
      if (editor && p.kind === 'num') { if (p.min != null) editor.min=p.min; if (p.max != null) editor.max=p.max; editor.step=p.id==='TransferSize'?'1024':'1'; }
    });
  };
  if (ACTIVE_DOC) renderDoc(ACTIVE_DOC);
})();
