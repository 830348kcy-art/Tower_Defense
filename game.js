// Canvas renderer + input bridge for the Blazor tower-defense port.
// All gameplay lives in C#; this file only draws the scene returned by Frame(dt)
// each animation frame and forwards pointer/keyboard input back to .NET.

const KIND_FILES = [
  "normal", "fast", "splitbody", "splitsmall", "elite", "elitecharge",
  "eliteregenerator", "elitewyvern", "midbossnormal", "midbosscharge",
  "midbosssplit", "midbossspeed", "bossnormal", "bosscharge", "bosssplit", "bossspeed",
];

const state = {
  canvas: null,
  ctx: null,
  dotnet: null,
  raf: 0,
  last: 0,
  bg: null,            // offscreen background canvas
  sprites: [],
  damages: [],         // floating damage numbers {x,y,vy,life,text,color,size}
  announce: { text: "", time: 0 },
  ended: false,
  baseHref: "",
};

function spriteUrl(file) {
  return new URL(`assets/enemies/${file}.png`, document.baseURI).href;
}

function preloadSprites() {
  state.sprites = KIND_FILES.map((f) => {
    const img = new Image();
    img.src = spriteUrl(f);
    return img;
  });
}

function dmgColor(type) {
  switch (type) {
    case 1: return "#AA82FF"; // Magic
    case 2: return "#FF821E"; // Explosive
    case 3: return "#FFDC00"; // True
    default: return "#FFFFFF";
  }
}

// ── Public API ──────────────────────────────────────────────────────────
export function init(dotnetRef, canvasId, baseHref) {
  state.canvas = document.getElementById(canvasId);
  state.ctx = state.canvas.getContext("2d");
  state.dotnet = dotnetRef;
  state.baseHref = baseHref || "";
  state.ended = false;
  state.damages = [];
  state.announce = { text: "", time: 0 };
  preloadSprites();

  state.canvas.addEventListener("click", onClick);
  state.keyHandler = (e) => {
    if (state.dotnet) state.dotnet.invokeMethodAsync("OnKey", e.key);
  };
  window.addEventListener("keydown", state.keyHandler);

  state.last = performance.now();
  state.raf = requestAnimationFrame(loop);
}

export function setMap(map) {
  const bg = document.createElement("canvas");
  bg.width = map.width;
  bg.height = map.height;
  const g = bg.getContext("2d");

  // base fill
  g.fillStyle = "#1B2733";
  g.fillRect(0, 0, map.width, map.height);

  // tiles
  for (const t of map.tiles) {
    g.fillStyle = t.color;
    g.fillRect(t.x, t.y, map.tileSize - 1, map.tileSize - 1);
    g.strokeStyle = "rgba(0,0,0,0.2)";
    g.lineWidth = 0.5;
    g.strokeRect(t.x, t.y, map.tileSize - 1, map.tileSize - 1);
  }

  if (map.nightOverlay) {
    g.fillStyle = "rgba(0,0,0,0.28)";
    g.fillRect(0, 0, map.width, map.height);
  }

  // path lines
  g.save();
  g.globalAlpha = 0.5;
  g.strokeStyle = map.pathColor;
  g.lineWidth = 34;
  g.lineCap = "round";
  g.lineJoin = "round";
  for (const l of map.pathLines) {
    g.beginPath();
    g.moveTo(l.x1, l.y1);
    g.lineTo(l.x2, l.y2);
    g.stroke();
  }
  g.restore();

  // spawn markers
  for (const s of map.spawns) {
    g.fillStyle = "crimson";
    g.beginPath();
    g.moveTo(s.x, s.y - 14);
    g.lineTo(s.x - 12, s.y + 8);
    g.lineTo(s.x + 12, s.y + 8);
    g.closePath();
    g.fill();
    g.fillStyle = "white";
    g.font = "bold 11px sans-serif";
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.fillText("S", s.x, s.y);
  }

  // base markers
  for (const b of map.bases) {
    g.fillStyle = "royalblue";
    roundRect(g, b.x - 17, b.y - 17, 34, 34, 5);
    g.fill();
    g.strokeStyle = "white";
    g.lineWidth = 2;
    g.stroke();
    g.font = "17px sans-serif";
    g.textAlign = "center";
    g.textBaseline = "middle";
    g.fillText("🏰", b.x, b.y);
  }

  state.bg = bg;
}

export function stop() {
  cancelAnimationFrame(state.raf);
  state.raf = 0;
  if (state.canvas) state.canvas.removeEventListener("click", onClick);
  if (state.keyHandler) window.removeEventListener("keydown", state.keyHandler);
}

// ── Input ───────────────────────────────────────────────────────────────
function toLogical(e) {
  const rect = state.canvas.getBoundingClientRect();
  return {
    x: (e.clientX - rect.left) * (state.canvas.width / rect.width),
    y: (e.clientY - rect.top) * (state.canvas.height / rect.height),
  };
}

function onClick(e) {
  if (!state.dotnet) return;
  const p = toLogical(e);
  state.dotnet.invokeMethodAsync("OnClick", p.x, p.y);
}

// ── Loop ────────────────────────────────────────────────────────────────
function loop(ts) {
  const dt = Math.min((ts - state.last) / 1000, 0.05);
  state.last = ts;

  let scene = null;
  try {
    scene = state.dotnet.invokeMethod("Frame", dt);
  } catch (err) {
    console.error("Frame error", err);
    return;
  }

  draw(scene, dt);

  if (scene && scene.result !== 0 && !state.ended) {
    state.ended = true;
    state.dotnet.invokeMethodAsync("OnGameEnded", scene.result);
  }

  state.raf = requestAnimationFrame(loop);
}

// ── Draw ────────────────────────────────────────────────────────────────
function draw(scene, dt) {
  const ctx = state.ctx;
  const W = state.canvas.width;
  const H = state.canvas.height;
  ctx.clearRect(0, 0, W, H);
  if (state.bg) ctx.drawImage(state.bg, 0, 0);
  if (!scene) return;

  // effects (under everything)
  for (const fx of scene.effects) {
    ctx.globalAlpha = fx.alpha;
    ctx.fillStyle = fx.color;
    ctx.beginPath();
    ctx.arc(fx.x, fx.y, fx.r, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1;

  // range ring
  if (scene.ring) {
    ctx.beginPath();
    ctx.arc(scene.ring.x, scene.ring.y, scene.ring.r, 0, Math.PI * 2);
    ctx.fillStyle = "rgba(255,255,0,0.10)";
    ctx.fill();
    ctx.strokeStyle = "rgba(255,220,0,0.78)";
    ctx.lineWidth = 1.5;
    ctx.stroke();
  }

  // soldiers
  for (const s of scene.soldiers) {
    if (!s.alive) continue;
    ctx.fillStyle = s.rein ? "goldenrod" : "steelblue";
    ctx.strokeStyle = "white";
    ctx.lineWidth = 1;
    ctx.fillRect(s.x - 6.5, s.y - 6.5, 13, 13);
    ctx.strokeRect(s.x - 6.5, s.y - 6.5, 13, 13);
    ctx.fillStyle = "limegreen";
    ctx.fillRect(s.x - 6.5, s.y - 14, 13 * s.hp, 3);
  }

  // enemies
  for (const e of scene.enemies) {
    const img = state.sprites[e.kind];
    ctx.globalAlpha = e.status === 1 ? 0.65 : e.status === 2 ? 0.85 : 1;
    if (img && img.complete && img.naturalWidth > 0) {
      ctx.drawImage(img, e.x - e.size / 2, e.y - e.size / 2, e.size, e.size);
    } else {
      ctx.fillStyle = e.color;
      ctx.beginPath();
      ctx.arc(e.x, e.y, e.size / 2.4, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.globalAlpha = 1;

    // hp bar
    const bx = e.x - e.barW / 2;
    const by = e.y - e.size / 2 - 9;
    ctx.fillStyle = "rgb(30,30,30)";
    ctx.fillRect(bx, by, e.barW, 5);
    ctx.fillStyle = hpColor(e.hp);
    ctx.fillRect(bx, by, e.barW * e.hp, 5);
  }

  // towers
  for (const t of scene.towers) {
    ctx.fillStyle = t.color;
    roundRect(ctx, t.x - 19, t.y - 19, 38, 38, 7);
    ctx.fill();
    ctx.strokeStyle = "rgb(20,20,20)";
    ctx.lineWidth = 1.5;
    ctx.stroke();

    ctx.font = "18px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(t.icon, t.x, t.y);

    // level pips / branch badge
    if (t.branch) {
      ctx.fillStyle = "white";
      ctx.font = "bold 8px sans-serif";
      ctx.fillText(t.branch, t.x, t.y + 14);
    } else {
      const total = t.maxLevel;
      const gap = 7;
      const startX = t.x - ((total - 1) * gap) / 2;
      for (let i = 0; i < total; i++) {
        ctx.beginPath();
        ctx.arc(startX + i * gap, t.y + 14, 2.5, 0, Math.PI * 2);
        ctx.fillStyle = i <= t.level ? "gold" : "rgba(60,60,60,0.47)";
        ctx.fill();
      }
    }
  }

  // projectiles
  for (const p of scene.projectiles) {
    const hs = p.big ? 5 : 3.5;
    ctx.fillStyle = p.color;
    ctx.beginPath();
    ctx.arc(p.x, p.y, hs, 0, Math.PI * 2);
    ctx.fill();
  }

  // floating damage numbers
  for (const d of scene.damages) {
    state.damages.push({
      x: d.x + (d.crit ? 0 : 0), y: d.y - 22,
      life: 0.8, text: d.crit ? "⚡" + Math.round(d.amount) + "!" : "" + Math.round(d.amount),
      color: dmgColor(d.type), size: d.crit ? 18 : 12,
    });
  }
  ctx.textAlign = "center";
  ctx.textBaseline = "middle";
  for (let i = state.damages.length - 1; i >= 0; i--) {
    const d = state.damages[i];
    d.life -= dt;
    if (d.life <= 0) { state.damages.splice(i, 1); continue; }
    d.y -= 45 * dt;
    ctx.globalAlpha = Math.max(0, d.life / 0.8);
    ctx.fillStyle = d.color;
    ctx.font = `${d.size}px sans-serif`;
    ctx.fillText(d.text, d.x, d.y);
  }
  ctx.globalAlpha = 1;

  // boss hp bar
  if (scene.bossName) {
    const bw = W - 220, bx = 110, by = 8;
    ctx.fillStyle = "rgba(34,0,0,0.8)";
    ctx.fillRect(bx, by, bw, 18);
    ctx.fillStyle = "#FF1744";
    ctx.fillRect(bx, by, bw * scene.bossHp, 18);
    ctx.fillStyle = "#FF5252";
    ctx.font = "bold 12px sans-serif";
    ctx.textAlign = "left";
    ctx.fillText(scene.bossName, bx + 6, by + 9);
  }

  // wave announce
  if (scene.newWave > 0) {
    const last = scene.newWave === scene.totalWaves;
    state.announce.text = last
      ? `⚠ 최후의 웨이브!  (${scene.newWave} / ${scene.totalWaves})`
      : `웨이브 ${scene.newWave} / ${scene.totalWaves}`;
    state.announce.time = 2.5;
  }
  if (state.announce.time > 0) {
    state.announce.time -= dt;
    ctx.globalAlpha = state.announce.time > 0.5 ? 1 : Math.max(0, state.announce.time / 0.5);
    ctx.fillStyle = "rgba(0,0,0,0.55)";
    ctx.fillRect(0, H / 2 - 34, W, 68);
    ctx.fillStyle = "#FFD369";
    ctx.font = "bold 30px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(state.announce.text, W / 2, H / 2);
    ctx.globalAlpha = 1;
  }

  // top HUD strip
  drawHud(ctx, scene, W);

  // result banner
  if (scene.result !== 0) {
    ctx.fillStyle = "rgba(0,0,0,0.6)";
    ctx.fillRect(0, 0, W, H);
    ctx.fillStyle = scene.result === 1 ? "#7CFF6B" : "#FF6B6B";
    ctx.font = "bold 54px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(scene.result === 1 ? "승리!" : "패배", W / 2, H / 2 - 20);
  }
}

function drawHud(ctx, scene, W) {
  ctx.save();
  ctx.fillStyle = "rgba(34,40,49,0.9)";
  ctx.fillRect(0, 0, W, 0); // strip handled by HTML; keep numeric overlay minimal
  ctx.font = "bold 15px sans-serif";
  ctx.textBaseline = "top";
  ctx.textAlign = "left";

  ctx.fillStyle = "#FFD369";
  ctx.fillText(`♦ ${scene.gold}G`, 10, 34);
  ctx.fillStyle = "#E63946";
  ctx.fillText(`❤ ${scene.lives} / ${scene.livesMax}`, 110, 34);
  ctx.fillStyle = "#EEEEEE";
  ctx.fillText(`웨이브 ${scene.wave} / ${scene.totalWaves}`, 250, 34);

  ctx.fillStyle = "#9FE6FF";
  const cd = scene.countdown < 0 ? "마지막 웨이브" : `다음 웨이브: ${scene.countdown.toFixed(1)}s`;
  ctx.fillText(cd, 400, 34);
  ctx.restore();
}

function hpColor(ratio) {
  const r = ratio > 0.5 ? Math.round(255 * (1 - ratio) * 2) : 220;
  const g = ratio < 0.5 ? Math.round(200 * ratio * 2) : 190;
  return `rgb(${r},${g},0)`;
}

function roundRect(ctx, x, y, w, h, r) {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}
