// Convierte los SVG del manual a imágenes de alta resolución para el PDF.
const { chromium } = require('playwright');
const fs = require('fs'), path = require('path');
(async () => {
  const dir = path.join(__dirname, 'svg'), out = path.join(__dirname, '..', 'img');
  fs.mkdirSync(out, { recursive: true });
  const b = await chromium.launch();
  const p = await b.newPage({ deviceScaleFactor: 2 });
  for (const f of fs.readdirSync(dir).filter(f => f.endsWith('.svg'))) {
    const svg = fs.readFileSync(path.join(dir, f), 'utf8');
    const m = svg.match(/width="(\d+)" height="(\d+)"/);
    await p.setViewportSize({ width: +m[1], height: +m[2] });
    await p.setContent(`<html><body style="margin:0;background:transparent">${svg}</body></html>`);
    const nombre = f.replace('.svg', '');
    const png = nombre === 'marco' || nombre === 'd20';
    await p.screenshot({ path: path.join(out, nombre + (png ? '.png' : '.jpg')), type: png ? 'png' : 'jpeg', quality: png ? undefined : 86, omitBackground: png });
  }
  await b.close();
})();
