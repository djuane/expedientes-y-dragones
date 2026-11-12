const { chromium } = require('playwright');
const fs = require('fs'), path = require('path');
(async () => {
  const b = await chromium.launch();
  const p = await b.newPage();
  const dir = path.join(__dirname, 'svg');
  const salida = path.join(__dirname, '..', 'cliente-unity', 'Assets', 'Resources', 'arte');
  // Tipografías del juego (OFL), las mismas que usa Unity.
  const fuentes = path.join(__dirname, '..', 'cliente-unity', 'Assets', 'Resources', 'fuentes');
  const css = [['Alegreya', 'Alegreya-Bold.ttf'], ['Alegreya Italic', 'Alegreya-Italic.ttf'], ['Cinzel Decorative', 'CinzelDecorative-Bold.ttf']]
    .map(([f, a]) => `@font-face{font-family:'${f}';src:url(data:font/ttf;base64,${fs.readFileSync(path.join(fuentes, a)).toString('base64')}) format('truetype');}`).join('');
  // portada-vectorial es la portada anterior hecha por código: queda como fuente, no entra al juego.
  for (const f of fs.readdirSync(dir).filter(f => f.endsWith('.svg') && f !== 'portada-vectorial.svg')) {
    const svg = fs.readFileSync(path.join(dir, f), 'utf8');
    const w = +svg.match(/width="(\d+)"/)[1], h = +svg.match(/height="(\d+)"/)[1];
    await p.setViewportSize({ width: w, height: h });
    await p.setContent(`<html><head><style>${css}</style></head><body style="margin:0;background:transparent">${svg}</body></html>`);
    await p.evaluate(() => document.fonts.ready);
    await p.screenshot({ path: path.join(salida, f.replace('.svg', '.png')), omitBackground: true, clip: { x: 0, y: 0, width: w, height: h } });
  }
  await b.close();
})();
