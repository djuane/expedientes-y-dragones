using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Logica.Investigacion
{
    /// <summary>
    /// Casos del modo investigación. Pasan en la República de Expedientia, un país inventado: sus horarios y reglamentos
    /// son de ficción y no explican normas reales ni son asesoría legal. Lo jurídico real va en las pruebas jurídicas verificadas.
    /// </summary>
    public static class CasosDeInvestigacion
    {
        public const string Bermudez = "Lic. Bermúdez";
        public const string Tecnico = "Jhony, el técnico";
        public const string Tintorera = "Doña Felícita";

        private static LineaDeGuion L(string p, string t) => new LineaDeGuion(p, t);
        private static LineaDeGuion[] Ls(params LineaDeGuion[] l) => l;
        private const string N = Guion.Narrador, G = Guion.Gonzabio, Li = Guion.Lion, C = Guion.Cliente, R = Guion.Rosita,
            E = Guion.Eusebio, V = Guion.Valdivia, B = Bermudez, K = Guion.Coimero;

        public static readonly CasoDeInvestigacion EscritoFantasma = new CasoDeInvestigacion(
            "escrito-fantasma", 1, "El escrito fantasma",
            "El cliente apeló el último día, antes del cierre. Ahora dicen que llegó tarde y que perdió el juicio.",
            horas: 9,
            introduccion: Ls(
                L(N, "Expedientia, lunes, 8:00 a. m. La audiencia es a las 5 de la tarde. El estudio tiene nueve horas."),
                L(C, "Doctor, yo presenté mi apelación el último día, antes de que cerraran. ¡Lo juro por mi madre!"),
                L(C, "Ahora dicen que llegó al día siguiente y que perdí el juicio. Tenía el cargo… bueno, lo tenía hasta el lunes."),
                L(G, "Aquí está la resolución: «improcedente por extemporánea». Según el sistema, el escrito ingresó a las 9:10 del día siguiente."),
                L(Li, "Si el papel llegó a tiempo, alguien dejó huellas. Cada diligencia nos cuesta una hora, así que elijan bien adónde ir."),
                L(G, "Cuando tengamos con qué, armamos el escrito y vamos a la audiencia. Si se nos pierde el rumbo, pídanle una pista a Lion.")),
            pruebas: new[]
            {
                new Prueba("resolucion", "Resolución de improcedencia", "Declara improcedente la apelación: según el sistema, ingresó a las 9:10 del día siguiente al último día del plazo."),
                new Prueba("fotocopia", "Fotocopia borrosa del cargo", "La copia de Gonzabio. Se lee el número de expediente, pero la hora es una mancha."),
                new Prueba("tintoreria", "El terno en la tintorería", "La esposa del cliente mandó el terno gris a la tintorería «La Impecable»… con todo lo que tenía en los bolsillos.", presentable: false),
                new Prueba("cargo", "Cargo original de recepción", "Sello de la Ventanilla 3 con fecha del último día del plazo y hora 16:55."),
                new Prueba("aviso", "Aviso de horario de la Ventanilla 3", "«Atención al público: de 8:00 a 17:00.» Con sello de la Jefatura de Mesa de Partes y fecha de este año."),
                new Prueba("bitacora", "Bitácora del técnico de turno", "«Caída total del sistema de 16:40 a 17:35. Se avisó a Jefatura.» Firmada por el técnico."),
                new Prueba("cuaderno", "Cuaderno de entrega de sellos", "El sello de la Ventanilla 3 se entregó a Rosita a las 8:00 de ese mismo día. Entrega: Lic. Bermúdez (firma)."),
                new Prueba("bono", "El bono de cero incidencias", "Don Eusebio cuenta que Jefatura cobra un bono si reporta «cero incidencias» del sistema.", presentable: false),
                new Prueba("memorando", "Memorando de la papelera", "«Lo recibido durante caídas se registra al día siguiente. Reportar cero incidencias. El bono depende de esto.» Firma: Bermúdez."),
                new Prueba("constancia", "Constancia «agilizada»", "Un papel que dice que el escrito llegó a tiempo. No tiene número, ni firma, ni registro. Lo vendió el Coimero.", viciada: true),
            },
            pruebasIniciales: new[] { "resolucion", "fotocopia" },
            lugares: new[]
            {
                new Lugar("estudio", "El estudio", "Un cuarto alquilado frente a la Corte. Huele a café y a tóner.", "fondo-mesa", new[]
                {
                    new Hallazgo("estudio-gonzabio", "Repasar el caso con Gonzabio", Ls(
                        L(G, "La apelación vencía el viernes. El cliente dice que la dejó en la Ventanilla 3 antes de las cinco."),
                        L(G, "El sistema dice lunes 9:10. Uno de los dos miente, y los sistemas no cobran bonos… ¿o sí?")), horas: 0),
                    new Hallazgo("estudio-lion", "Leer el expediente con Lion", Ls(
                        L(Li, "Página 47: el sistema registra la hora en que alguien teclea el ingreso, no la hora en que llegó el papel."),
                        L(Li, "Si ese viernes nadie tecleó nada en la tarde… quiero saber por qué.")), horas: 1),
                }),
                new Lugar("casa", "Casa del cliente", "Una casa con más santos que muebles. La esposa del cliente plancha mientras escucha.", "escena-pasillo", new[]
                {
                    new Hallazgo("casa-cliente", "Preguntarle por el cargo", Ls(
                        L(C, "El cargo lo guardé en el bolsillo del terno gris, el de las audiencias. De eso estoy seguro."),
                        L(C, "¿Y el terno? Mi esposa lo… ¡Hermelinda! ¿Dónde está mi terno gris?"),
                        L(N, "Desde la cocina: «En la tintorería La Impecable, pues. Estaba lleno de papeles, ni los saqué».")), da: "tintoreria", clave: true),
                    new Hallazgo("casa-papeles", "Revisar sus papeles", Ls(
                        L(N, "Recibos de luz, una estampita, la garantía de una licuadora de 1998."),
                        L(G, "Aquí no hay nada. Perdimos una hora, pero la licuadora sigue en garantía… moralmente.")), horas: 1),
                }),
                new Lugar("tintoreria", "Tintorería «La Impecable»", "Vapor, perchas y una señora que lo recuerda todo.", "escena-libreria", requiere: "tintoreria", hallazgos: new[]
                {
                    new Hallazgo("tintoreria-terno", "Preguntar por el terno gris", Ls(
                        L(Tintorera, "¿El terno gris del señor? Aquí está. Yo nunca lavo un saco sin revisar los bolsillos, joven."),
                        L(Tintorera, "Tenía este papel con sello. Lo guardé en un sobre, por si era importante."),
                        L(G, "¡El cargo original! Sello de la Ventanilla 3… viernes, 16:55. Cinco minutos antes del cierre.")), da: "cargo", clave: true),
                }),
                new Lugar("mesa", "Mesa de Partes", "Seis ventanillas, una que funciona. La cola da la vuelta al pasillo.", "escena-tramite", new[]
                {
                    new Hallazgo("mesa-avisos", "Leer los avisos de la Ventanilla 3", Ls(
                        L(N, "Entre un afiche de vacunación y uno de «No se aceptan escritos con grapas», hay un aviso con sello:"),
                        L(N, "«Atención al público: de 8:00 a 17:00.» Fecha de este año."),
                        L(Li, "Lo voy a fotografiar. Los avisos tienen la mala costumbre de desaparecer.")), da: "aviso", clave: true),
                    new Hallazgo("mesa-rosita", "Hablar con Rosita", Ls(
                        L(R, "¿El viernes? Yo recibo lo que llega, doctor. Todo termina en la cola."),
                        L(R, "No me haga hablar, que tengo tres hijos y un jefe."),
                        L(Li, "Está asustada. Si le mostramos que ya sabemos algo, quizás se anime.")), horas: 1),
                    new Hallazgo("mesa-rosita-bitacora", "Mostrarle la bitácora a Rosita", Ls(
                        L(R, "…Ya saben lo de la caída. Está bien."),
                        L(R, "Ese viernes recibí en papel y sellé a mano, como siempre que se cae el sistema. El sello me lo entregaron en la mañana."),
                        L(R, "Aquí está el cuaderno de sellos. Mire quién firmó la entrega."),
                        L(G, "Firma: Lic. Bermúdez. El jefe de Mesa de Partes.")), da: "cuaderno", requiere: "bitacora", clave: true),
                }),
                new Lugar("soporte", "Sótano de soporte técnico", "Cables, ventiladores y un técnico que no ve el sol desde 2019.", "fondo-contrataciones", new[]
                {
                    new Hallazgo("soporte-tecnico", "Hablar con el técnico de turno", Ls(
                        L(Tecnico, "¿El viernes? Uy, se cayó todo. De 16:40 a 17:35. Lo anoté en la bitácora, como manda el manual."),
                        L(Tecnico, "Avisé a Jefatura, pero me dijeron que «no hubo incidencias». Yo solo anoto, doctor."),
                        L(Li, "Una caída que no existe para el jefe, pero sí para la bitácora. Me la llevo, con su permiso.")), da: "bitacora", clave: true),
                }),
                new Lugar("pasillo", "Pasillo del juzgado", "Donde de verdad se tramitan las cosas: entre el café y los rumores.", "escena-cafeteria", new[]
                {
                    new Hallazgo("pasillo-eusebio", "Tomar un café con Don Eusebio", Ls(
                        L(E, "¿Bermúdez? Doce años de jefe y nunca reporta una falla. ¿Sabe por qué, doctor?"),
                        L(E, "Por el bono de «cero incidencias». Si el sistema se cae, el bono se cae con él."),
                        L(E, "Dicen que guarda todo en su oficina. Y que nunca vacía la papelera.")), da: "bono", clave: true),
                    new Hallazgo("pasillo-coimero", "Escuchar al señor amable del pasillo", Ls(
                        L(K, "Doctor, por una propina le consigo una constancia de que su escrito llegó a tiempo. Nadie pregunta."),
                        L(N, "El estudio acepta el papel. Gonzabio lo mira como quien mira un billete falso."),
                        L(G, "Esto no lo voy a foliar en ningún expediente mío. Ojalá no tengamos que usarlo.")), da: "constancia", atajo: true),
                }),
                new Lugar("oficina", "Oficina del Lic. Bermúdez", "Diplomas, una planta de plástico y una papelera que no se vacía.", "escena-audiencia", requiere: "bono", hallazgos: new[]
                {
                    new Hallazgo("oficina-papelera", "Revisar la papelera", Ls(
                        L(N, "Bermúdez salió a almorzar. La puerta quedó entreabierta y la secretaria dice: «Pasen, si es por un trámite»."),
                        L(Li, "Un memorando arrugado: «Lo recibido durante caídas se registra al día siguiente. Reportar cero incidencias»."),
                        L(Li, "Y al pie: «El bono depende de esto». Firma: Bermúdez. Ah, y tiene un sello raro… del Archivo Central.")), da: "memorando", clave: true),
                }),
            },
            pretensiones: new[]
            {
                new Pretension("Que se tenga la apelación por presentada a tiempo, con el cargo de las 16:55, y se deje sin efecto la improcedencia.", true, Ls(
                    L(G, "Eso. Pedimos lo que el cliente necesita: que su apelación cuente. Lo demás, después."))),
                new Pretension("Que se sancione a Mesa de Partes y se le pague una indemnización al cliente.", false, Ls(
                    L(G, "Ambicioso… pero eso no salva la apelación, que es lo urgente. La jueza va a fruncir el ceño."),
                    L(V, "Doctor, esto no es lo que corresponde discutir hoy. Lo tendré presente."))),
                new Pretension("Que se le dé al cliente un plazo nuevo para volver a apelar.", false, Ls(
                    L(Li, "Pedir un plazo nuevo es admitir que llegamos tarde. Y no llegamos tarde."),
                    L(V, "¿Entonces el estudio acepta que el escrito fue extemporáneo? Curioso. Lo tendré presente."))),
            },
            antesDeLaAudiencia: Ls(
                L(N, "5:00 p. m. Sala de audiencias. La Doctora Valdivia ajusta sus lentes."),
                L(V, "Se instala la audiencia. El estudio sostiene que la apelación se presentó a tiempo. Llamo al testigo."),
                L(Li, "Escuchen cada frase. Cuando una no cuadre con lo que tenemos, presenten la prueba. Si no están seguros, repregunten.")),
            testimonios: new[]
            {
                new Testimonio(B, "Sobre el día del plazo", Ls(L(B, "Con mucho gusto, señora jueza. Seré breve: aquí no hay misterio.")), new[]
                {
                    new LineaDeTestimonio("Soy jefe de Mesa de Partes hace doce años. Conozco cada sello y cada escrito.", Ls(
                        L(B, "Cada uno, doctor. Y el suyo no llegó a tiempo."))),
                    new LineaDeTestimonio("Ese viernes el sistema funcionó perfectamente, sin una sola falla.", Ls(
                        L(B, "Perfectamente. Lo reporté así a la Gerencia, como todos los días."),
                        L(Li, "«Como todos los días». Nadie tiene tantos días perfectos.")), "bitacora", Ls(
                        L(G, "Bitácora del técnico de turno: caída total del sistema de 16:40 a 17:35. Firmada y con aviso a Jefatura."),
                        L(B, "Eh… una caidita. Nada importante."),
                        L(V, "Para usted será nada. Para esta apelación puede ser todo. Testigo, corrija su declaración."))),
                    new LineaDeTestimonio("El escrito ingresó el lunes a las 9:10, como dice el sistema.", Ls(
                        L(B, "El sistema no miente, doctor."),
                        L(Li, "El sistema registra cuándo alguien teclea el ingreso, no cuándo llega el papel."))),
                    new LineaDeTestimonio("Por lo tanto, la apelación es extemporánea. No hay más que decir.", Ls(
                        L(B, "Nada más. ¿Ya me puedo ir?"))),
                }),
                new Testimonio(B, "Sobre el horario", Ls(L(B, "Está bien, hubo una caída. Pero eso no cambia nada: a esa hora ya no se atendía.")), new[]
                {
                    new LineaDeTestimonio("Mesa de Partes atiende hasta las 16:30. Así está establecido.", Ls(
                        L(B, "Hasta las 16:30. Hace años que es así."),
                        L(G, "En la ventanilla había un aviso con el horario. ¿Alguien lo leyó?")), "aviso", Ls(
                        L(G, "Aviso pegado en la propia Ventanilla 3: «Atención al público: de 8:00 a 17:00». Con el sello de su Jefatura."),
                        L(B, "¡Ese aviso es viejo!"),
                        L(V, "Tiene fecha de este año, testigo. Y su sello. Siga."))),
                    new LineaDeTestimonio("Después del cierre no se recibe ni un papel.", Ls(
                        L(B, "Ni uno. Las reglas son las reglas."))),
                    new LineaDeTestimonio("Si el señor vino tarde, que no culpe a la institución.", Ls(
                        L(C, "¡Yo no vine tarde!"),
                        L(V, "Silencio en la sala."))),
                }),
                new Testimonio(B, "Sobre la hora de llegada", Ls(L(B, "Bien, hasta las cinco. Pero el señor llegó pasadas las cinco. Me lo informaron.")), new[]
                {
                    new LineaDeTestimonio("El señor llegó a la ventanilla a las 17:20, cuando ya estaba cerrada.", Ls(
                        L(B, "17:20. Me lo informaron mis ventanilleras."),
                        L(Li, "Si recibieron el papel, tuvo que quedar un cargo. Siempre queda un cargo.")), "cargo", Ls(
                        L(C, "¡Mi cargo! ¡Estaba en el bolsillo del terno!"),
                        L(G, "Cargo original con el sello de la Ventanilla 3: viernes, 16:55. Cinco minutos antes del cierre."),
                        L(V, "Testigo, el sello dice otra hora que usted."))),
                    new LineaDeTestimonio("A las cinco en punto mis ventanilleras se van. Nadie se queda.", Ls(
                        L(B, "Nadie. Ni un minuto más."),
                        L(R, "(desde la galería) …a veces sí nos quedamos.")) ),
                    new LineaDeTestimonio("Eso es todo lo que sé, señora jueza.", Ls(
                        L(V, "Lo dudo, testigo."))),
                }),
                new Testimonio(B, "Sobre el sello", Ls(L(B, "¡Ese sello no vale! El sello de la Ventanilla 3 se perdió hace meses. Cualquiera pudo usarlo.")), new[]
                {
                    new LineaDeTestimonio("Cualquiera con un sello robado pudo inventar ese cargo.", Ls(
                        L(B, "Hay mucha gente inescrupulosa en los pasillos, doctor."),
                        L(G, "En eso tiene razón. Pero no es nuestro caso."))),
                    new LineaDeTestimonio("Ese sello se perdió en marzo. Desde entonces nadie lo tiene.", Ls(
                        L(B, "Se reportó la pérdida… de palabra."),
                        L(Li, "Los sellos se entregan cada mañana con firma. Rosita lo sabe.")), "cuaderno", Ls(
                        L(G, "Cuaderno de entrega de sellos: el de la Ventanilla 3 se entregó a Rosita a las 8:00 de ese mismo viernes."),
                        L(Li, "¿Y quién firma la entrega? Usted, licenciado."),
                        L(V, "Testigo, le recuerdo que declara bajo juramento."))),
                    new LineaDeTestimonio("Mi oficina no tiene nada que esconder.", Ls(
                        L(B, "Nada. Pueden revisar lo que quieran."),
                        L(Li, "Qué curioso que lo diga."))),
                }),
                new Testimonio(B, "Sobre el registro", Ls(L(B, "Aun si recibieron el papel, el sistema manda. Y yo no tengo nada que ver con lo que se registra.")), new[]
                {
                    new LineaDeTestimonio("El bono de productividad no tiene nada que ver con este caso.", Ls(
                        L(B, "¿Qué bono? Digo… ese bono es para todo el personal."))),
                    new LineaDeTestimonio("Yo jamás di la orden de registrar nada con otra fecha.", Ls(
                        L(B, "Jamás. Mis ventanilleras registran lo que llega, cuando llega."),
                        L(Li, "Rosita estaba muy asustada. Alguien le dio una orden.")), "memorando", Ls(
                        L(G, "Memorando de su papelera: «Lo recibido durante caídas se registra al día siguiente. Reportar cero incidencias»."),
                        L(Li, "Y al pie: «El bono depende de esto». Con su firma."),
                        L(B, "¡El bono era para todos!"),
                        L(V, "Basta, testigo."))),
                    new LineaDeTestimonio("Yo solo cumplo las metas que me pone la institución.", Ls(
                        L(B, "Metas, doctor. Números. Así se mide el trabajo."),
                        L(G, "Y así se pierden los juicios de la gente."))),
                }),
            },
            victoria: Ls(
                L(V, "Queda acreditado que el escrito se recibió el viernes a las 16:55, dentro del horario. Se tiene la apelación por presentada a tiempo."),
                L(V, "Y remito copias a control interno por lo del memorando. Se levanta la audiencia."),
                L(C, "¡Doctor! ¡Doctores! No sé cómo pagarles… bueno, sí sé, pero denme hasta fin de mes."),
                L(G, "Un expediente bien armado aguanta cualquier golpe. Hoy aguantó."),
                L(Li, "Una cosa más: el memorando tiene un sello del Archivo Central. Alguien más arriba quería cifras limpias.")),
            derrota: Ls(
                L(V, "El estudio no ha acreditado su versión. La apelación sigue siendo improcedente. Se levanta la audiencia."),
                L(C, "¿Y ahora qué hago, doctor?"),
                L(G, "Nos faltaron pruebas. La próxima vez, más calle y menos café."),
                L(Li, "Volvamos a empezar. Esta vez sabemos dónde mirar.")));

        public static readonly IReadOnlyList<CasoDeInvestigacion> Todos = new[] { EscritoFantasma };

        public static CasoDeInvestigacion Buscar(string id) => Todos.First(c => c.Id == id);
    }
}
