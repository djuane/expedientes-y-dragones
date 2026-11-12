# Pone título, autor y fechas al PDF del manual (sin datos del programa que lo generó).
import sys, datetime
from pypdf import PdfReader, PdfWriter

r = PdfReader('salida/manual-bruto.pdf')
w = PdfWriter(clone_from=r)
fecha = datetime.datetime.now(datetime.timezone.utc).strftime('D:%Y%m%d%H%M%SZ')
w.metadata = None
w.add_metadata({'/Title': 'Expedientes y Dragones · Manual del juego', '/Author': 'Leonel Díaz', '/Subject': 'Reglas, especialidades, bestiario y aventura',
                '/Keywords': 'Expedientes y Dragones; juego de rol; manual', '/Creator': 'Leonel Díaz', '/Producer': 'Leonel Díaz',
                '/CreationDate': fecha, '/ModDate': fecha})
w.compress_identical_objects()
with open('salida/Expedientes-y-Dragones-Manual.pdf', 'wb') as f:
    w.write(f)
import os; os.remove('salida/manual-bruto.pdf')
