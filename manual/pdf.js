// Genera el PDF del manual a partir de manual.html y avisa si algún contenido se desborda de su página.
const { chromium } = require('playwright');
const path = require('path');
(async () => {
  const b = await chromium.launch();
  const p = await b.newPage();
  await p.goto('file://' + path.join(__dirname, 'manual.html'), { waitUntil: 'networkidle' });
  await p.evaluate(() => document.fonts.ready);
  const problemas = await p.evaluate(() => {
    const out = [];
    document.querySelectorAll('.pag').forEach(pg => {
      const c = pg.querySelector('.contenido');
      const r = c.getBoundingClientRect();
      const dos = c.querySelectorAll('.dos');
      dos.forEach(d => { if (d.scrollWidth > d.clientWidth + 2) out.push(pg.dataset.n + ': columnas desbordadas ' + (d.scrollWidth - d.clientWidth)); });
      c.querySelectorAll(':scope > *, .esp-txt, .bloque').forEach(e => {
        const er = e.getBoundingClientRect();
        if (er.bottom > r.bottom + 2) out.push(pg.dataset.n + ': ' + (e.className || e.tagName) + ' baja ' + Math.round(er.bottom - r.bottom) + 'px');
      });
      c.querySelectorAll('.esp-txt, .bloque').forEach(e => { if (e.scrollHeight > e.clientHeight + 2) out.push(pg.dataset.n + ': ' + e.className + ' recortado ' + (e.scrollHeight - e.clientHeight)); });
    });
    return out;
  });
  console.log(problemas.length ? problemas.join('\n') : 'Sin desbordes');
  await p.pdf({ path: path.join(__dirname, 'salida', 'manual-bruto.pdf'), width: '210mm', height: '297mm', printBackground: true, preferCSSPageSize: true });
  await b.close();
})();
