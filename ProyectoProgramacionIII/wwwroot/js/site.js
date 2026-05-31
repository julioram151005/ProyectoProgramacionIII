const API = '/api/Archive';

const categories = {
  'Programas': { icon: '💻' },
  'Documentos': { icon: '📄' },
  'Juegos': { icon: '🎮' },
  'Películas': { icon: '🎬' },
  'Imágenes': { icon: '🖼️' },
  'Audio': { icon: '🎵' },
  'Otros': { icon: '📦' }
};

let files = [];
let queueLocal = [];

const $ = s => document.querySelector(s);
const $$ = s => document.querySelectorAll(s);

const fmt = b => {
  if (!b) return '0 B';
  const u = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(b) / Math.log(1024));
  return `${(b / Math.pow(1024, i)).toFixed(i ? 1 : 0)} ${u[i]}`;
};

function metaKey(id){ return 'zipstore_meta_' + id; }
function getMeta(id){ try { return JSON.parse(localStorage.getItem(metaKey(id)) || '{}'); } catch { return {}; } }
function saveMeta(id, meta){ localStorage.setItem(metaKey(id), JSON.stringify(meta || {})); }

function guessCategory(name){
  const n = (name || '').toLowerCase();
  if (n.includes('pelicula') || n.includes('película') || n.includes('movie') || n.includes('video')) return 'Películas';
  if (n.includes('imagen') || n.includes('foto') || n.includes('png') || n.includes('jpg')) return 'Imágenes';
  if (n.includes('audio') || n.includes('musica') || n.includes('música') || n.includes('mp3')) return 'Audio';
  if (n.includes('juego') || n.includes('game') || n.includes('steam') || n.includes('minecraft') || n.includes('gta')) return 'Juegos';
  if (n.includes('programa') || n.includes('setup') || n.includes('instalador') || n.includes('install') || n.includes('rufus') || n.includes('odin') || n.includes('driver')) return 'Programas';
  if (n.includes('documento') || n.includes('doc') || n.includes('pdf') || n.includes('excel') || n.includes('word') || n.includes('arbol') || n.includes('reporte') || n.includes('tarea')) return 'Documentos';
  return 'Otros';
}

function normalizedFile(a){
  const id = a.id ?? a.Id;
  const originalName = a.nombreOriginal ?? a.NombreOriginal ?? a.nombre ?? a.Nombre ?? 'Archivo.zip';
  const size = a.tamanoBytes ?? a.TamanoBytes ?? 0;
  const date = a.fechaSubida ?? a.FechaSubida;
  const mime = a.tipoMime ?? a.TipoMime ?? '';
  const hash = a.hashMd5 ?? a.HashMd5 ?? '';
  const meta = getMeta(id);
  return {
    id,
    name: meta.title || originalName,
    originalName,
    size,
    date,
    mime,
    hash,
    description: meta.description || 'Archivo almacenado en la base de datos.',
    category: categories[meta.category] ? meta.category : guessCategory(originalName)
  };
}

function iconOf(c){ return categories[c]?.icon || '📦'; }
function slug(text){ return (text || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/\s+/g,'-'); }
function escapeHtml(str){ return String(str || '').replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#039;'}[m])); }

function toast(msg){
  const t = $('#toast');
  if(!t) return;
  t.textContent = msg;
  t.classList.add('show');
  setTimeout(() => t.classList.remove('show'), 2600);
}

async function api(path, opts = {}){
  const r = await fetch(API + path, opts);
  if(!r.ok){
    let m = await r.text();
    throw new Error(m || 'Error');
  }
  return r.headers.get('content-type')?.includes('application/json') ? r.json() : r;
}

function openSection(id){
  $$('.view').forEach(v => v.classList.remove('active'));
  $('#' + id)?.classList.add('active');
  $$('.nav-link').forEach(b => b.classList.toggle('active', b.dataset.section === id));
  window.scrollTo(0, 0);
}

$$('[data-section]').forEach(b => b.onclick = () => openSection(b.dataset.section));
document.addEventListener('click', e => {
  const b = e.target.closest('[data-section-open]');
  if(b) openSection(b.dataset.sectionOpen);
});

$('#themeToggle')?.addEventListener('click', () => {
  document.body.classList.toggle('light');
  $('#themeToggle').textContent = document.body.classList.contains('light') ? '☀' : '☾';
});

$('#searchForm')?.addEventListener('submit', e => {
  e.preventDefault();
  renderSearchResults();
});
$('#searchInput')?.addEventListener('input', () => {
  const q = $('#searchInput').value.trim();
  if(q.length >= 2) renderSearchResults();
  if(!q) $('#searchResults')?.classList.add('hidden');
});

$('#categoryFilter')?.addEventListener('change', renderCatalog);
$('#sortFilter')?.addEventListener('change', renderCatalog);
$('#categoryFilterUpload')?.addEventListener('change', renderUploadCatalog);
$('#sortFilterUpload')?.addEventListener('change', renderUploadCatalog);

async function loadFiles(){
  try{ files = await api('/list'); }
  catch(e){ files = []; toast('No se pudo conectar con la API. Revisa que el proyecto esté corriendo.'); }
  renderAll();
}

function allNormalized(){ return files.map(normalizedFile); }

function filtered(filterId = 'categoryFilter', sortId = 'sortFilter'){
  const q = $('#searchInput')?.value.toLowerCase() || '';
  let arr = allNormalized().filter(f => (f.name + ' ' + f.originalName + ' ' + f.category + ' ' + f.description + ' ' + f.hash).toLowerCase().includes(q));
  const cf = $('#' + filterId)?.value;
  if(cf && cf !== 'Todos') arr = arr.filter(f => f.category === cf);
  const sort = $('#' + sortId)?.value;
  if(sort === 'nombre') arr.sort((a,b) => a.name.localeCompare(b.name));
  if(sort === 'tamano') arr.sort((a,b) => b.size - a.size);
  if(sort === 'recientes') arr.sort((a,b) => new Date(b.date || 0) - new Date(a.date || 0));
  return arr;
}

function renderSearchResults(){
  const box = $('#searchResults');
  if(!box) return;
  const q = $('#searchInput')?.value.trim().toLowerCase() || '';
  if(!q){ box.classList.add('hidden'); return; }
  const arr = allNormalized().filter(f => (f.name + ' ' + f.originalName + ' ' + f.category + ' ' + f.description + ' ' + f.hash).toLowerCase().includes(q));
  box.classList.remove('hidden');
  box.innerHTML = `<b>${arr.length} resultado(s) encontrado(s).</b><div class="search-list">${arr.map(f => `
    <div class="search-item">
      <div><b>${escapeHtml(f.name)}</b><br><small>${f.category} · ${fmt(f.size)}</small></div>
      <button class="ghost" onclick="showDetail(${f.id})">Ver</button>
      <button class="primary" onclick="downloadFile(${f.id})">Descargar</button>
    </div>`).join('') || '<p class="muted">No se encontraron archivos.</p>'}</div>`;
}

function renderCategories(){
  const normalized = allNormalized();
  const counts = Object.keys(categories).map(c => ({ c, n: normalized.filter(f => f.category === c).length }));
  const html = counts.map(x => `<div class="cat-card" onclick="setCategoryAndOpen('${x.c}')"><span class="ico">${iconOf(x.c)}</span><b>${x.c}</b><small>${x.n} archivos</small></div>`).join('');
  if($('#categoryCards')) $('#categoryCards').innerHTML = html;
  const options = '<option value="Todos">Todas las categorías</option>' + Object.keys(categories).map(c => `<option>${c}</option>`).join('');
  if($('#categoryFilter')) $('#categoryFilter').innerHTML = options;
  if($('#categoryFilterUpload')) $('#categoryFilterUpload').innerHTML = options;
}

function setCategoryAndOpen(cat){
  if($('#categoryFilter')) $('#categoryFilter').value = cat;
  openSection('catalogo');
  renderCatalog();
}

function fileCard(f){
  const c = f.category;
  return `<article class="file-card" onclick="showDetail(${f.id})">
    <div class="poster ${slug(c)}">${iconOf(c)}</div>
    <div class="file-info">
      <div class="file-title">${escapeHtml(f.name)}</div>
      <div class="meta"><span>${c}</span><span>${fmt(f.size)}</span></div>
      <div class="stars">★ ★ ★ ★ ☆ <small style="color:#96a6bf">ID ${f.id}</small></div>
    </div>
  </article>`;
}

function renderCatalog(){
  if($('#catalogGrid')) $('#catalogGrid').innerHTML = filtered('categoryFilter','sortFilter').map(fileCard).join('') || '<p class="muted">No hay archivos en esta categoría.</p>';
}

function renderUploadCatalog(){
  if($('#uploadCatalogGrid')) $('#uploadCatalogGrid').innerHTML = filtered('categoryFilterUpload','sortFilterUpload').map(fileCard).join('') || '<p class="muted">No hay archivos en esta categoría.</p>';
}

function renderFeatured(){
  if($('#featuredGrid')) $('#featuredGrid').innerHTML = allNormalized().slice(0,5).map(fileCard).join('') || '<p class="muted">Sube archivos para verlos aquí.</p>';
}

function renderTable(){
  if(!$('#filesTable')) return;
  $('#filesTable').innerHTML = allNormalized().map(f => `<tr>
      <td>${escapeHtml(f.name)}</td>
      <td><select class="category-inline" onchange="changeCategory(${f.id}, this.value)">${Object.keys(categories).map(c => `<option ${c === f.category ? 'selected' : ''}>${c}</option>`).join('')}</select></td>
      <td>${fmt(f.size)}</td>
      <td>${f.date ? new Date(f.date).toLocaleString() : ''}</td>
      <td>
        <button class="action-btn download" title="Descargar" onclick="downloadFile(${f.id})">⬇</button>
        <button class="action-btn view" title="Ver detalles" onclick="showDetail(${f.id})">👁</button>
        <button class="action-btn delete" title="Eliminar" onclick="deleteFile(${f.id})">🗑</button>
      </td>
    </tr>`).join('') || '<tr><td colspan="5">Sin archivos.</td></tr>';
}

function renderStorage(){
  const total = allNormalized().reduce((s,f) => s + Number(f.size || 0), 0);
  const limit = 10 * 1024 * 1024 * 1024;
  const pct = Math.min(100, total / limit * 100);
  if($('#storageBar')) $('#storageBar').style.width = pct + '%';
  if($('#storageText')) $('#storageText').textContent = `${fmt(total)} de 10 GB`;
}

function renderMiniTree(){
  const box = $('#miniTree');
  if(!box) return;
  const normalized = allNormalized();
  box.innerHTML = `<div class="tree-center"><span class="root-node">▦ Raíz</span></div><div class="tree-categories">${Object.keys(categories).filter(c => normalized.some(f => f.category === c)).map(c => {
    const arr = normalized.filter(f => f.category === c);
    return `<div class="tree-cat"><h4><span>${iconOf(c)} ${c}</span><span class="badge">${arr.length}</span></h4>${arr.map(f => `<div class="tree-file">▧ ${escapeHtml(f.name.replace(/\.(zip|rar)$/i,''))}</div>`).join('')}</div>`;
  }).join('') || '<p class="muted">Sube archivos para construir el árbol.</p>'}</div>`;
}

function renderQueue(){
  if(!$('#downloadQueue')) return;
  $('#downloadQueue').innerHTML = queueLocal.map(f => `<div class="download-item"><div><b>${escapeHtml(f.name)}</b><div class="bar"><i style="width:${f.progress}%"></i></div></div><span>${f.progress}%</span><button class="action-btn download" title="Descargar" onclick="downloadFile(${f.id})">⬇</button></div>`).join('') || '<p class="muted">No hay descargas en cola.</p>';
}

function renderAll(){
  renderCategories();
  renderFeatured();
  renderCatalog();
  renderUploadCatalog();
  renderTable();
  renderStorage();
  renderMiniTree();
  renderQueue();
}

function changeCategory(id, category){
  const f = allNormalized().find(x => String(x.id) === String(id));
  const oldMeta = getMeta(id);
  saveMeta(id, { ...oldMeta, category, title: oldMeta.title || f?.name, description: oldMeta.description || f?.description });
  toast('Categoría actualizada a ' + category);
  renderAll();
}

function showDetail(id){
  const f = allNormalized().find(x => String(x.id) === String(id));
  if(!f) return;
  const c = f.category;
  $('#detailBox').innerHTML = `<div class="detail-poster">${iconOf(c)}</div><div>
      <h1>${escapeHtml(f.name)}</h1>
      <p><b>Categoría:</b></p>
      <select id="detailCategory">${Object.keys(categories).map(cat => `<option ${cat === c ? 'selected' : ''}>${cat}</option>`).join('')}</select>
      <p style="margin-top:12px"><b>Tamaño:</b> ${fmt(f.size)}</p>
      <p><b>Formato:</b> ${(f.originalName.split('.').pop() || 'archivo').toUpperCase()}</p>
      <p><b>Fecha:</b> ${f.date ? new Date(f.date).toLocaleString() : ''}</p>
      <p><b>Hash MD5:</b><br><small>${f.hash || 'Sin hash'}</small></p>
      <p class="muted">${escapeHtml(f.description)}</p>
      <div class="detail-actions">
        <button class="primary" onclick="downloadFile(${id})">⇩ Descargar</button>
        <button class="ghost" onclick="changeCategory(${id}, document.querySelector('#detailCategory').value); showDetail(${id});">Guardar categoría</button>
        <button class="danger" onclick="deleteFile(${id})">🗑 Eliminar</button>
        <button class="ghost" onclick="enqueueFile(${id})">Agregar a cola</button>
      </div>
    </div>`;
  openSection('detalle');
}

function downloadFile(id){
  enqueueFile(id, true);
  window.location.href = `${API}/download/${id}`;
}

async function deleteFile(id){
  if(!confirm('¿Eliminar archivo?')) return;
  try{
    await api(`/delete/${id}`, { method:'DELETE' });
    localStorage.removeItem(metaKey(id));
    toast('Archivo eliminado');
    await loadFiles();
    openSection('catalogo');
  }catch(e){ toast('Error al eliminar: ' + e.message); }
}

async function enqueueFile(id, silent = false){
  try{ await api('/cola/enqueue', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({ idArchivo:id }) }); }catch{}
  const f = allNormalized().find(x => String(x.id) === String(id));
  if(f && !queueLocal.some(x => String(x.id) === String(id))) queueLocal.push({ ...f, progress: Math.floor(20 + Math.random() * 70) });
  renderQueue();
  if(!silent) toast('Agregado a cola');
}

$('#uploadForm')?.addEventListener('submit', async e => {
  e.preventDefault();
  const input = $('#fileInput');
  if(!input.files.length) return toast('Selecciona un archivo');
  const selected = input.files[0];
  if(!/\.(zip|rar)$/i.test(selected.name)){
    toast('Solo se permiten archivos .zip o .rar');
    input.value = '';
    return;
  }
  const fd = new FormData();
  fd.append('file', selected);
  try{
    const result = await api('/upload', { method:'POST', body:fd });
    const id = result.id || result.Id;
    if(id){
      saveMeta(id, {
        category: $('#uploadCategory')?.value || 'Otros',
        title: $('#uploadTitle')?.value || selected.name,
        description: $('#uploadDescription')?.value || 'Archivo almacenado en la base de datos.'
      });
    }
    toast('Archivo subido correctamente');
    input.value = '';
    if($('#uploadTitle')) $('#uploadTitle').value = '';
    if($('#uploadDescription')) $('#uploadDescription').value = '';
    await loadFiles();
    openSection('catalogo');
  }catch(err){ toast('Error al subir: ' + err.message); }
});

$('#fileInput')?.addEventListener('change', e => {
  if(e.target.files[0]){
    if(!/\.(zip|rar)$/i.test(e.target.files[0].name)){
      toast('Solo se permiten archivos .zip o .rar');
      e.target.value = '';
      return;
    }
    if($('#uploadTitle')) $('#uploadTitle').value = e.target.files[0].name;
  }
});

$$('[data-order]').forEach(btn => {
  btn.addEventListener('click', async () => {
    const order = btn.dataset.order;
    const result = $('#verifyResult');
    result.textContent = `Ejecutando ${btn.textContent}...`;
    try{
      const data = await api(`/arbol/${order}`);
      const arr = (data.archivos || []).map(normalizedFile);
      result.innerHTML = `<b>${data.recorrido}</b><br>${arr.map(f => escapeHtml(f.name)).join(' → ') || 'Sin archivos'}`;
    }catch(e){
      const arr = allNormalized();
      result.innerHTML = `<b>${btn.textContent}</b><br>${arr.map(f => escapeHtml(f.name)).join(' → ') || 'Sin archivos'}`;
    }
  });
});

loadFiles();
