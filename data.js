const CONFIG = {
  gridSize: 5,
  levels: 10,
  baseCoinPerTick: 1,
  tickMs: 1000,
  crateCost: 50,
  spawnCostCurve: lvl => 10 * Math.pow(1.35, lvl-1),
  worlds: [
    { id: 'earth', name: 'Origins', color:'#20314a', worldMult: 1.0 },
    { id: 'ember', name: 'Ember Vale', color:'#402531', worldMult: 1.4 },
    { id: 'aether', name: 'Aether Reef', color:'#233b40', worldMult: 2.0 },
  ],
  shardsPerAscend: (coinsTotal, bestLevel) => {
    const fromCoins = Math.floor(Math.max(0, Math.log10(1 + coinsTotal) - 2));
    const fromLevel = Math.max(0, Math.floor((bestLevel - 5)/2));
    return fromCoins + fromLevel;
  },
  godUnlockAtShards: 12,
  godAuraPerOffering: 0.25,
};

const SHOP_DEF = [
  { id:'spawnTier', name:'Evolved spawns', desc:'Spawn higher base level creatures.', price: 120, step:1, cap:5 },
  { id:'betterCrates', name:'Better crates', desc:'Crates drop higher levels.', price: 150, step:1, cap:4 },
  { id:'prodBoost', name:'Amplify fields', desc:'+15% passive production per level.', price: 180, step:1, cap:10 },
  { id:'extraDrops', name:'Echo spawn', desc:'15% chance to drop a bonus creature.', price: 250, step:1, cap:3 },
  { id:'autoSpawnSpeed', name:'Faster auto‑spawn', desc:'Reduce auto‑spawn delay by 1 second.', price: 200, step:1, cap:9 }
];
