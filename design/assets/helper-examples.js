/* Browser interaction previews. Production API calls run in the native demo. */
"use strict";
(function(){
  function text(key){return window.CodeWFCatalogText[LANG][key]||key;}
  function register(api,render){
    var entry=window.CodeWFDemoCatalog.flatMap(function(g){return g.items;}).find(function(e){return e.api===api;});
    REG(entry.prototypeId,{zh:{t:entry.titleZh,d:entry.package,f:[]},en:{t:entry.titleEn,d:entry.package,f:[]},cs:entry.code,props:[],render:render});
  }
  ['I18nManager','JsonLocalizer'].forEach(function(api){register(api,function(el){
    var input=api==='JsonLocalizer'?'zh-CN':'Demo.Item.Welcome';
    el.innerHTML='<div class="demo"><p>'+esc(text('LocalizationHint'))+'</p><input class="catalog-input" value="'+esc(input)+'"><button class="btn pri">'+esc(text('Run'))+'</button></div><div class="demo"><h4>'+esc(text('Output'))+'</h4><pre class="catalog-result">'+esc(text('Ready'))+'</pre></div>';
    el.querySelector('button').onclick=function(){
      var value=el.querySelector('input').value.trim(),result;
      if(api==='I18nManager')result='Culture = '+LANG+'\nLanguages = zh-CN, en-US\n'+value+' = '+(value==='Demo.Item.Welcome'?(LANG==='zh-CN'?'介绍':'Introduction'):value);
      else result='Culture = '+value+'\n'+(value.toLowerCase()==='zh-cn'?'必须指定 GitHub 仓库名（UpdateServiceOptions.Repository）。':'Specify a GitHub repository (UpdateServiceOptions.Repository).');
      el.querySelector('.catalog-result').textContent=result+'\n'+text('Preview');
    };
  });});
  register('UpdateService',function(el){
    var state='idle',timer=null;
    el.innerHTML='<div class="demo"><p>'+esc(text('UpdateFixtureHint'))+'</p><input class="catalog-input" value="Hello update"><div class="row"><button class="btn pri" data-check>'+esc(text('UpdateCheck'))+'</button><button class="btn" data-download>'+esc(text('UpdateDownload'))+'</button><button class="btn" data-cancel>'+esc(text('UpdateCancel'))+'</button></div><progress max="100" value="0"></progress><pre class="catalog-result">'+esc(text('Ready'))+'</pre></div>';
    function paint(){el.querySelector('[data-download]').disabled=state!=='available';el.querySelector('[data-check]').disabled=state==='downloading';el.querySelector('[data-cancel]').disabled=state!=='downloading';}
    el.querySelector('[data-check]').onclick=function(){state='available';el.querySelector('progress').value=0;el.querySelector('.catalog-result').textContent=text('UpdateAvailable')+'\n'+text('Preview');paint();};
    el.querySelector('[data-download]').onclick=function(){state='downloading';paint();var progress=el.querySelector('progress');progress.value=0;timer=setInterval(function(){if(!el.isConnected){clearInterval(timer);return;}progress.value+=10;if(progress.value>=100){clearInterval(timer);state='downloaded';el.querySelector('.catalog-result').textContent=text('UpdateVerified');paint();}},80);};
    el.querySelector('[data-cancel]').onclick=function(){clearInterval(timer);state='available';el.querySelector('.catalog-result').textContent=text('UpdateCanceled');paint();};paint();
  });
  register('DataTableExtensions',function(el){
    el.innerHTML='<div class="demo"><p>'+esc(text('TableHint'))+'</p><textarea class="catalog-input">Ada,42\nLin,24</textarea><div class="row"><button class="btn" data-format="CSV">'+esc(text('TableCsv'))+'</button><button class="btn" data-format="Excel">'+esc(text('TableExcel'))+'</button></div></div><div class="demo"><h4>'+esc(text('Output'))+'</h4><table class="catalog-grid"><thead><tr><th>Name</th><th>Value</th></tr></thead><tbody></tbody></table><p class="catalog-result">'+esc(text('Ready'))+'</p></div>';
    el.querySelectorAll('[data-format]').forEach(function(button){button.onclick=function(){
      var rows=el.querySelector('textarea').value.trim().split('\n').map(function(l){return l.split(',');});
      if(!rows.length||rows.some(function(c){return c.length!==2;})){el.querySelector('.catalog-result').textContent=text('TableFormat');return;}
      el.querySelector('tbody').innerHTML=rows.map(function(c){return '<tr><td>'+esc(c[0])+'</td><td>'+esc(c[1])+'</td></tr>';}).join('');
      el.querySelector('.catalog-result').textContent=button.dataset.format+' · Rows = '+rows.length+'\n'+text('Preview');
    };});
  });
  register('ActionHelper',function(el){
    el.innerHTML='<div class="demo"><p>'+esc(text('ActionFormat'))+'</p><input class="catalog-input" value="20,100"><button class="btn pri">'+esc(text('Run'))+'</button><p class="catalog-result"></p></div>';
    el.querySelector('button').onclick=function(){var values=el.querySelector('input').value.split(',').map(Number),valid=values.length===2&&values.every(function(n){return Number.isInteger(n)&&n>=0&&n<=5000;});el.querySelector('.catalog-result').textContent=valid?'CheckOvertime = '+(values[0]<=values[1])+'\n'+text(values[0]<=values[1]?'ActionCompleted':'ActionTimedOut'):text('ActionFormat');};
  });
  ['TcpSocketClientFileSystemFeature','TcpSocketServerFileSystemFeature'].forEach(function(api){register(api,function(el){
    var running=false,folder=false,uploaded=false,busy=false,timer=null;
    el.innerHTML='<div class="demo"><h4>'+esc(text('FileSession'))+'</h4><p>'+esc(text(api.includes('Server')?'FileServerHint':'FileClientHint'))+'</p><div class="row"><button class="btn pri" data-op="start">'+esc(text('FileStart'))+'</button><button class="btn" data-op="stop">'+esc(text('FileStop'))+'</button></div></div><div class="demo"><h4>'+esc(text('FileOperations'))+'</h4><div class="row">'+[['browse','FileBrowse'],['create','FileCreate'],['delete','FileDelete'],['upload','FileUpload'],['download','FileDownload'],['cancel','FileCancel']].map(function(pair){return '<button class="btn" data-op="'+pair[0]+'">'+esc(text(pair[1]))+'</button>';}).join('')+'</div><progress max="100" value="0"></progress><table class="catalog-grid"><thead><tr><th>Name</th><th>Path</th></tr></thead><tbody></tbody></table></div><div class="demo"><pre class="catalog-result">'+esc(text('Preview'))+'</pre></div>';
    function append(s){el.querySelector('.catalog-result').textContent+='\n'+s;}
    function paint(){
      el.querySelectorAll('[data-op]').forEach(function(button){var op=button.dataset.op;button.disabled=op==='start'?running:op==='stop'?!running:op==='cancel'?!busy:!running||busy;});
      el.querySelector('tbody').innerHTML=(running?['sample.txt'].concat(folder?['demo-folder']:[]).concat(uploaded?['payload.bin']:[]):[]).map(function(name){return '<tr><td>'+name+'</td><td>session/server/'+name+'</td></tr>';}).join('');
    }
    el.querySelectorAll('[data-op]').forEach(function(button){button.onclick=function(){
      var op=button.dataset.op;
      if(op==='start'){running=true;append(text('TransportStarted'));}
      if(op==='stop'){running=false;busy=false;clearInterval(timer);append(text('FileStopped'));}
      if(op==='create'){folder=true;append('CreateDirectoryAsync: demo-folder');}
      if(op==='delete'){folder=false;append('DeletePathAsync: demo-folder');}
      if(op==='browse')append('BrowseFileSystemAsync');
      if(op==='cancel'){clearInterval(timer);busy=false;append(text('FileCancelled'));}
      if(op==='upload'||op==='download'){
        if(op==='download'&&!uploaded){append('payload.bin: '+text('Ready'));return;}
        busy=true;var progress=el.querySelector('progress');progress.value=0;
        timer=setInterval(function(){if(!el.isConnected){clearInterval(timer);return;}progress.value+=10;if(progress.value>=100){clearInterval(timer);busy=false;if(op==='upload')uploaded=true;append((op==='upload'?'UploadFileAsync':'DownloadFileAsync')+' · '+text('Preview'));paint();}},80);
      }
      paint();
    };});paint();
  });});
  ['EventServer','EventClient','TcpSocketServer','TcpSocketClient','UdpSocketServer','UdpSocketClient'].forEach(function(api){register(api,function(el){
    var running=false,subscribed=true,event=api.startsWith('Event');
    el.innerHTML='<div class="demo"><h4>'+esc(text('FileSession'))+'</h4><p>'+esc(text('TransportHint'))+'</p><div class="row"><button class="btn pri" data-start>'+esc(text('FileStart'))+'</button><button class="btn" data-stop>'+esc(text('FileStop'))+'</button><span class="catalog-connected"></span></div></div><div class="demo"><h4>'+esc(text('Input'))+'</h4><input class="catalog-input" value="Hello CodeWF"><div class="row"><button class="btn" data-send>'+esc(text('TransportSend'))+'</button>'+(event?'<label><input type="checkbox" data-subscribe checked> '+esc(text('TransportSubscribe'))+'</label>':'')+'</div></div><div class="demo"><pre class="catalog-result">'+esc(text('Preview'))+'</pre></div>';
    function append(s){el.querySelector('.catalog-result').textContent+='\n'+s;}
    function paint(){el.querySelector('[data-start]').disabled=running;el.querySelector('[data-stop]').disabled=!running;el.querySelector('[data-send]').disabled=!running;el.querySelector('.catalog-connected').textContent=text('TransportConnected')+' = '+running;}
    el.querySelector('[data-start]').onclick=function(){running=true;append(text('TransportStarted'));paint();};
    el.querySelector('[data-stop]').onclick=function(){running=false;append(text('FileStopped'));paint();};
    el.querySelector('[data-send]').onclick=function(){append(event&&!subscribed?text('TransportNoSubscription'):'Sent = '+el.querySelector('.catalog-input').value+'\nReceived = '+el.querySelector('.catalog-input').value);};
    if(event)el.querySelector('[data-subscribe]').onchange=function(e){subscribed=e.target.checked;};paint();
  });});
  REG('flow-runtime',{zh:{t:pt('flow-runtime'),d:text('RuntimeTourHint'),f:[]},props:[],render:function(el){
    var steps=text('RuntimeTourSteps').split('|'),routes=['bus','prism','bus','aspnet','log-core'];
    el.innerHTML='<div class="demo"><p>'+esc(text('RuntimeTourHint'))+'</p><div class="row">'+steps.map(function(step,i){return '<button class="btn" data-tour="'+i+'">'+(i+1)+' · '+esc(step)+'</button>';}).join('')+'</div></div><div class="catalog-tour-page"></div>';
    function show(index){el.querySelectorAll('[data-tour]').forEach(function(button){button.classList.toggle('pri',Number(button.dataset.tour)===index);});renderInto(routes[index],el.querySelector('.catalog-tour-page'));}
    el.querySelectorAll('[data-tour]').forEach(function(button){button.onclick=function(){show(Number(button.dataset.tour));};});show(0);
  }});
  if(ACTIVE_DOC)renderDoc(ACTIVE_DOC);
})();
