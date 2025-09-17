// game.js
let state = {
  coins: 0,
  coinsTotal: 0,
  shards: 0,
  globalMult: 1.0,
  godAura: 1.0,
  activeWorld: 'earth',
  worlds: {},
  upgrades: { spawnTier: 1, betterCrates: 0, prodBoost: 0, extraDrops: 0, autoSpawnSpeed: 0 },
  ascensions: 0,
  autoSpawnInterval: 10000
};

function initWorldData(id){
  const size = CONFIG.gridSize * CONFIG.gridSize;
  if (!state.worlds[id]){
    state.worlds[id] = { cells: new Array(size).fill(0), spawnLevel: 1, boosts: 0, unlocked: id==='earth', bestLevel: 1 };
  }
}
function clamp(v,min,max){return Math.max(min,Math.min(max,v));}
function fmt(n){ return Math.abs(n) >= 1e6 ? n.toExponential(2) : Math.floor(n).toLocaleString(); }
function round2(n){ return Math.round(n*100)/100; }
function totalMult(worldId){
  const world = CONFIG.worlds.find(w=>w.id===worldId) || CONFIG.worlds[0];
  const prodBoost = 1 + state.upgrades.prodBoost*0.15;
  return world.worldMult * state.globalMult * state.godAura * prodBoost;
}

const gridEl = document.getElementById('grid');
const tabBar = document.getElementById('tabBar');
const panelTitle = document.getElementById('panelTitle');
const worldInfo = document.getElementById('worldInfo');
const coinsChip = document.getElementById('coinsChip');
const shardsChip = document.getElementById('shardsChip');
const multChip = document.getElementById('multChip');
const spawnBtn = document.getElementById('spawnBtn');
const boxBtn = document.getElementById('boxBtn');
const shopEl = document.getElementById('shop');
const ascendBtn = document.getElementById('ascendBtn');
const shardsPreview = document.getElementById('shardsPreview');
const godProg = document.getElementById('godProg');
const godCard = document.getElementById('godCard');
const godOfferBtn = document.getElementById('godOfferBtn');
const godPowerChip = document.getElementById('godPowerChip');
const saveBtn = document.getElementById('saveBtn');
const resetBtn = document.getElementById('resetBtn');
const hint = document.getElementById('hint');

function setup(){
  const saved = localStorage.getItem('evo_save_v1');
  if (saved){
    try{ state = Object.assign(state, JSON.parse(saved)); }catch(e){}
  }
  CONFIG.worlds.forEach(w => initWorldData(w.id));
  renderTabs();
  buildGrid();
  renderShop();
  setWorld(state.activeWorld);
  updateHUD();
  computeShardPreview();
  updateGodUI();
  updateAutoSpawnTimer();
}
setup();

function renderTabs(){
  tabBar.innerHTML = '';
  CONFIG.worlds.forEach(w=>{
    const b = document.createElement('button');
    b.className = 'tab' + (state.activeWorld===w.id ? ' active' : '');
    b.textContent = w.name;
    b.onclick = ()=>{
      if (!state.worlds[w.id].unlocked){
        const unlockedCount = Object.values(state.worlds).filter(x=>x.unlocked).length;
        const cost = 500 * (1 + Math.max(0, unlockedCount-1));
        if (state.coins >= cost){
          state.coins -= cost;
          state.worlds[w.id].unlocked = true;
        } else {
          alert(`Unlock costs ${fmt(cost)} coins.`);
          return;
        }
      }
      setWorld(w.id);
    };
    tabBar.appendChild(b);
  });
  const t = document.createElement('button');
  t.className = 'tab' + (state.activeWorld==='temple' ? ' active' : '');
  t.textContent = 'Temple';
  t.onclick = ()=> setWorld('temple');
  tabBar.appendChild(t);
}

function setWorld(id){
  state.activeWorld = id;
  if (id === 'temple'){
    panelTitle.textContent = 'Temple';
    gridEl.innerHTML = '';
    gridEl.classList.add('world');
    gridEl.style.background = '#131524';
    worldInfo.textContent = 'Offer shards, ascend, and reach godhood.';
    spawnBtn.style.display = 'none';
    boxBtn.style.display = 'none';
    hint.textContent = 'Ascend to earn shards based on total coins and highest evolution reached.';
  } else {
    const wcfg = CONFIG.worlds.find(w=>w.id===id);
    panelTitle.textContent = wcfg ? wcfg.name : 'World';
    spawnBtn.style.display = '';
    boxBtn.style.display = '';
    hint.textContent = 'Merge identical creatures by dragging one onto another to evolve them.';
    buildGrid();
    updateWorldInfo();
  }
  renderTabs();
  updateHUD();
}

function buildGrid(){
  const wid = state.activeWorld;
  if (wid==='temple') return;
  const wcfg = CONFIG.worlds.find(w=>w.id===wid);
  gridEl.innerHTML = '';
  gridEl.classList.add('world');
  gridEl.style.background = 'linear-gradient(180deg,'+(wcfg?wcfg.color:'#0f1422')+',#0f1422 70%)';
  const size = CONFIG.gridSize * CONFIG.gridSize;
  for (let i=0;i<size;i++){
    const cell = document.createElement('div');
    cell.className = 'cell';
    cell.dataset.idx = i;
    cell.addEventListener('dragover', ev=>ev.preventDefault());
    cell.addEventListener('drop', onDrop);
    gridEl.appendChild(cell);
  }
  renderEntities();
}

function renderEntities(){
  const wid = state.activeWorld;
  if (wid==='temple') return;
  const world = state.worlds[wid];
  [...gridEl.querySelectorAll('.entity')].forEach(e=>e.remove());
  [...gridEl.children].forEach((cell,i)=>{
    const lvl = world.cells[i]||0;
    if (lvl>0){
      const e = document.createElement('div');
      e.className = 'entity e-l'+lvl;
      e.textContent = 'L'+lvl;
      e.draggable = true;
      e.dataset.idx = i;
      e.dataset.level = lvl;
      e.addEventListener('dragstart', onDragStart);
      e.addEventListener('dragend', onDragEnd);
      cell.appendChild(e);
    }
  });
}

let dragSrc = null;
function onDragStart(ev){
  dragSrc = Number(ev.target.dataset.idx);
  ev.dataTransfer.setData('text/plain', dragSrc);
}
function onDragEnd(){ dragSrc = null; }
function onDrop(ev){
  ev.preventDefault();
  const src = Number(ev.dataTransfer.getData('text/plain'));
  const dst = Number(this.dataset.idx);
  if (isNaN(src) || isNaN(dst) || src===dst) return;
  const wid = state.activeWorld;
  const world = state.worlds[wid];
  const a = world.cells[src]||0;
  const b = world.cells[dst]||0;
  if (a===0) return;
  if (b===0){
    world.cells[dst] = a;
    world.cells[src] = 0;
  } else if (a===b){
    const next = clamp(a+1,1,CONFIG.levels);
    world.cells[dst] = next;
    world.cells[src] = 0;
    const reward = Math.floor(10 * a * totalMult(wid));
    addCoins(reward);
    world.bestLevel = Math.max(world.bestLevel, next);
    pulseCell(dst);
  } else {
    [world.cells[dst], world.cells[src]] = [a,b];
  }
  renderEntities();
  updateHUD();
  computeShardPreview();
}

function pulseCell(idx){
  const cell = gridEl.children[idx];
  if (!cell) return;
  cell.style.boxShadow = '0 0 0 2px #9f6cff55 inset, 0 0 18px #9f6cff66';
  setTimeout(()=>{ cell.style.boxShadow=''; }, 180);
}

let tickTimer = setInterval(tick, CONFIG.tickMs);
function tick(){
  CONFIG.worlds.forEach(w=>{
    const world = state.worlds[w.id];
    const sum = world.cells.reduce((acc,l)=> acc + (l||0), 0);
    const add = CONFIG.baseCoinPerTick * sum * totalMult(w.id);
    addCoins(add);
  });
}

function addCoins(amount){
  state.coins += amount;
  state.coinsTotal += amount;
  updateHUD();
}

function randomEmptyIndex(world){
  const empties = world.cells.map((v,i)=>v===0?i:-1).filter(i=>i>=0);
  if (empties.length===0) return -1;
  return empties[(Math.random()*empties.length)|0];
}
function spawn(level=1){
  const wid = state.activeWorld;
  if (wid==='temple') return;
  const world = state.worlds[wid];
  const idx = randomEmptyIndex(world);
  if (idx<0){ return; }
  world.cells[idx] = level;
  world.bestLevel = Math.max(world.bestLevel, level);
  if (state.upgrades.extraDrops>0 && Math.random() < 0.15*state.upgrades.extraDrops){
    const idx2 = randomEmptyIndex(world);
    if (idx2>=0) world.cells[idx2] = level;
  }
  renderEntities();
  updateHUD();
  computeShardPreview();
}

spawnBtn.onclick = ()=>{
  const baseLevel = clamp(state.upgrades.spawnTier,1,CONFIG.levels);
  const cost = Math.floor(CONFIG.spawnCostCurve(baseLevel));
  if (state.coins < cost){ alert(`Need ${fmt(cost)} coins.`); return; }
  state.coins -= cost;
  spawn(baseLevel);
};

boxBtn.onclick = ()=>{
  if (state.activeWorld==='temple') return;
  if (state.coins < CONFIG.crateCost){ alert('Not enough coins.'); return; }
  state.coins -= CONFIG.crateCost;
  const crateBonus = state.upgrades.betterCrates;
  const lvl = clamp(1 + Math.floor(Math.random()*(2+crateBonus)),1,CONFIG.levels);
  spawn(lvl);
};

function updateWorldInfo(){
  const w = CONFIG.worlds.find(x=>x.id===state.activeWorld);
  const world = state.worlds[state.activeWorld];
  const occ = world.cells.filter(x=>x>0).length;
  worldInfo.textContent = `World x${w.worldMult} • Occupied ${occ}/${CONFIG.gridSize*CONFIG.gridSize} • Best L${world.bestLevel}`;
}

function renderShop(){
  shopEl.innerHTML = '';
  SHOP_DEF.forEach(item=>{
    const level = state.upgrades[item.id]||0;
    const cost = Math.floor(item.price * Math.pow(1.7, level));
    const row = document.createElement('div');
    row.className = 'item';
    row.innerHTML = `
      <div>
        <div><strong>${item.name}</strong> <span class="subtle">Lv ${level}/${item.cap}</span></div>
        <div class="subtle">${item.desc}</div>
      </div>
      <div class="row">
        <div class="badge">${fmt(cost)}</div>
        <button class="btn" ${level>=item.cap?'disabled':''}>Buy</button>
      </div>`;
    const buyBtn = row.querySelector('button');
    buyBtn.onclick = ()=>{
      const currentLevel = state.upgrades[item.id]||0;
      if (currentLevel>=item.cap) return;
      if (state.coins < cost){ alert('Not enough coins.'); return; }
      state.coins -= cost;
      state.upgrades[item.id] = (currentLevel||0) + item.step;
      if (item.id === 'autoSpawnSpeed') {
        const newDelay = 10000 - (state.upgrades.autoSpawnSpeed * 1000);
        state.autoSpawnInterval = Math.max(1000, newDelay);
        updateAutoSpawnTimer();
      }
      renderShop();
      updateHUD();
    };
    shopEl.appendChild(row);
  });
}

function computeShardPreview(){
  const bestLevel = Math.max(...Object.values(state.worlds).map(w=>w.bestLevel||1));
  const shards = CONFIG.shardsPerAscend(state.coinsTotal, bestLevel);
  shardsPreview.textContent = fmt(shards);
  const prog = clamp((state.shards + shards) / CONFIG.godUnlockAtShards, 0, 1);
  godProg.style.width = (prog*100)+'%';
  updateGodUI();
}

ascendBtn.onclick = ()=>{
  const bestLevel = Math.max(...Object.values(state.worlds).map(w=>w.bestLevel||1));
  const shardsNow = CONFIG.shardsPerAscend(state.coinsTotal, bestLevel);
  if (shardsNow<=0){ alert('Grow more before ascending.'); return; }
  if (!confirm(`Ascend and reset worlds for ${fmt(shardsNow)} shards?`)) return;
  state.shards += shardsNow;
  state.ascensions += 1;
  state.coins = 0;
  state.coinsTotal = 0;
  CONFIG.worlds.forEach(w=>{
    state.worlds[w.id].cells = new Array(CONFIG.gridSize*CONFIG.gridSize).fill(0);
    state.worlds[w.id].bestLevel = 1;
    state.worlds[w.id].unlocked = (w.id==='earth');
  });
  state.upgrades.spawnTier = Math.max(1, Math.min(state.upgrades.spawnTier, 2));
  state.upgrades.betterCrates = 0;
  state.upgrades.prodBoost = 0;
  state.upgrades.extraDrops = 0;
  state.upgrades.autoSpawnSpeed = 0;
  state.autoSpawnInterval = 10000;
  updateAutoSpawnTimer();
  renderTabs();
  buildGrid();
  renderEntities();
  renderShop();
  updateHUD();
  computeShardPreview();
  updateGodUI();
};

document.getElementById('spendShardBtn').onclick = ()=>{
  if (state.shards<=0){ alert('No shards.'); return; }
  state.shards -= 1;
  state.globalMult = round2(state.globalMult * 1.10);
  updateHUD();
  updateGodUI();
};

function updateGodUI(){
  const unlocked = (state.shards >= CONFIG.godUnlockAtShards) || state.godAura>1;
  godCard.style.display = unlocked ? '' : 'none';
  godPowerChip.textContent = `God aura: x${round2(state.godAura).toFixed(2)}`;
}

godOfferBtn.onclick = ()=>{
  if (state.shards < 5){ alert('Need 5 shards for an offering.'); return; }
  state.shards -= 5;
  state.godAura = round2(state.godAura * (1 + CONFIG.godAuraPerOffering));
  updateHUD();
  updateGodUI();
};

function updateHUD(){
  coinsChip.textContent = `Coins: ${fmt(state.coins)}`;
  shardsChip.textContent = `Shards: ${fmt(state.shards)}`;
  const multWorld = state.activeWorld==='temple' ? 'earth' : state.activeWorld;
  multChip.textContent = `x${round2(totalMult(multWorld)).toFixed(2)}`;
  if (state.activeWorld!=='temple') updateWorldInfo();
  save();
}

function save(){
  localStorage.setItem('evo_save_v1', JSON.stringify(state));
}

saveBtn.onclick = save;

resetBtn.onclick = ()=>{
  if (!confirm('Hard reset and lose progress?')) return;
  localStorage.removeItem('evo_save_v1');
  location.reload();
};

document.addEventListener('keydown', (e)=>{
  if (state.activeWorld==='temple') return;
  if (e.key===' '){ e.preventDefault(); spawnBtn.click(); }
  if (e.key==='c'){ e.preventDefault(); boxBtn.click(); }
});

// Auto-spawn system
let autoSpawnTimer;
function autoSpawn(){
  if (state.activeWorld !== 'temple') {
    spawn(1);
  }
}
function updateAutoSpawnTimer(){
  clearInterval(autoSpawnTimer);
  autoSpawnTimer = setInterval(autoSpawn, state.autoSpawnInterval);
}
