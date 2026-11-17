/*
    ============================================================================
    InmobiliariaDB - DATOS DE DEMOSTRACION
    Trabajo Practico Integrador - Bases de Datos I - UTN FRC
    ============================================================================

    Este script es ADITIVO: no borra la base ni las tablas, solo agrega filas.
    Es IDEMPOTENTE: se puede ejecutar varias veces sin duplicar datos.

    Se ejecuta DESPUES del script de creacion, con la base ya creada.

    Carga:
      - 4 inmobiliarias nuevas (quedan 5 en total)
      - 46 direcciones
      - 20 personas entre propietarios, clientes y agentes
      - 32 inmuebles repartidos entre las inmobiliarias y las ciudades
      - 32 publicaciones en ARS y USD, con fechas escalonadas
        a lo largo del ultimo anio para que los filtros por periodo tengan sentido
      - 11 contratos vigentes
      - 11 facturas con su detalle

    IMPORTANTE: al insertar los contratos se disparan los triggers
    tr_Contrato_ActualizarDisponibilidad y tr_Contrato_SinSolapamiento, que
    dan de baja la publicacion y pasan el inmueble a Vendido o Alquilado.
    Esos estados NO se escriben a mano: los calcula la base de datos.
    ============================================================================
*/

USE InmobiliariaDB;
GO

SET NOCOUNT ON;
GO


--------------------------------------DIRECCIONES--------------------------------------

INSERT INTO Direccion(calle, numero, piso, departamento, codigo_postal, id_ciudad)
SELECT x.calle, x.numero, x.piso, x.depto, x.cp, c.id_ciudad
FROM (VALUES
 ('Constitución',1150,NULL,NULL,'X5800','Río Cuarto'),
 ('Río Primero',3190,NULL,NULL,'X5107','Mendiolaza'),
 ('Sobremonte',1910,'8','B','X5800','Río Cuarto'),
 ('Sobremonte',2300,NULL,NULL,'X5800','Río Cuarto'),
 ('Río de Janeiro',560,NULL,NULL,'X5105','Villa Allende'),
 ('Constitución',710,NULL,NULL,'X5800','Río Cuarto'),
 ('Av. Goycoechea',650,NULL,NULL,'X5105','Villa Allende'),
 ('Av. San Martín',2210,NULL,NULL,'X5220','Jesús María'),
 ('Av. Vélez Sarsfield',2210,NULL,NULL,'X5000','Córdoba'),
 ('Los Tilos',970,NULL,NULL,'X5111','Río Ceballos'),
 ('Constitución',3460,NULL,NULL,'X5800','Río Cuarto'),
 ('Av. Uruguay',3170,NULL,NULL,'X5152','Villa Carlos Paz'),
 ('Sobremonte',1220,NULL,NULL,'X5800','Río Cuarto'),
 ('Av. San Martín',3340,NULL,NULL,'X5111','Río Ceballos'),
 ('Libertad',3190,NULL,NULL,'X5152','Villa Carlos Paz'),
 ('Av. Edén',2290,NULL,NULL,'X5172','La Falda'),
 ('Entre Ríos',3420,'2','D','X5900','Villa María'),
 ('Entre Ríos',760,'11','D','X5900','Villa María'),
 ('Río Primero',4620,NULL,NULL,'X5107','Mendiolaza'),
 ('Río Primero',490,NULL,NULL,'X5107','Mendiolaza'),
 ('Av. Sabattini',1370,NULL,NULL,'X5900','Villa María'),
 ('Belgrano',1900,'1','D','X5186','Alta Gracia'),
 ('Av. España',4350,NULL,NULL,'X5800','Río Cuarto'),
 ('Independencia',2940,'6','D','X5000','Córdoba'),
 ('España',1850,NULL,NULL,'X5172','La Falda'),
 ('Los Álamos',340,NULL,NULL,'X5109','Unquillo'),
 ('Los Cedros',820,NULL,NULL,'X5152','Villa Carlos Paz'),
 ('Av. Hipólito Yrigoyen',4780,'5','C','X5000','Córdoba'),
 ('Los Tilos',2780,NULL,NULL,'X5111','Río Ceballos'),
 ('Av. Sabattini',3020,NULL,NULL,'X5900','Villa María'),
 ('España',2790,NULL,NULL,'X5172','La Falda'),
 ('Río Primero',4170,NULL,NULL,'X5107','Mendiolaza'),
 ('Av. San Martín',2590,NULL,NULL,'X5111','Río Ceballos'),
 ('Sobremonte',2530,NULL,NULL,'X5800','Río Cuarto'),
 ('Tucumán',4030,NULL,NULL,'X5166','Cosquín'),
 ('Los Nogales',3090,NULL,NULL,'X5107','Mendiolaza'),
 ('Los Tilos',2690,'9','C','X5111','Río Ceballos'),
 ('Av. Sarmiento',3390,NULL,NULL,'X5186','Alta Gracia'),
 ('Montevideo',3530,NULL,NULL,'X5000','Córdoba'),
 ('Tucumán',2890,NULL,NULL,'X5220','Jesús María'),
 ('Av. España',3750,NULL,NULL,'X5800','Río Cuarto'),
 ('España',630,'4','D','X5172','La Falda'),
 ('Los Álamos',370,NULL,NULL,'X5109','Unquillo'),
 ('Independencia',2910,'8','A','X5000','Córdoba'),
 ('España',910,NULL,NULL,'X5172','La Falda'),
 ('Av. Edén',160,NULL,NULL,'X5172','La Falda')
) x(calle,numero,piso,depto,cp,ciudad)
JOIN Ciudad c ON c.nombre = x.ciudad
WHERE NOT EXISTS (
 SELECT 1 FROM Direccion d
 WHERE d.calle = x.calle AND d.numero = x.numero AND d.id_ciudad = c.id_ciudad
   AND ISNULL(d.piso,'') = ISNULL(x.piso,'')
   AND ISNULL(d.departamento,'') = ISNULL(x.depto,'')
);
GO

--------------------------------------INMOBILIARIAS--------------------------------------

INSERT INTO Inmobiliaria(nombre, cuit, telefono, email, id_direccion)
SELECT v.nombre, v.cuit, v.telefono, v.email, d.id_direccion
FROM (VALUES
 ('Altos del Suquía Propiedades','30715558841','351-555-0202','info@altosdelsuquia.example','Constitución',1150),
 ('Sierras Chicas Inmobiliaria','30716649932','3543-555-0303','contacto@sierraschicas.example','Río Primero',3190),
 ('Patio Olmos Bienes Raíces','30717730123','351-555-0404','ventas@patioolmos.example','Sobremonte',1910),
 ('Lago Azul Propiedades','30718821204','3541-555-0505','hola@lagoazul.example','Sobremonte',2300)
) v(nombre,cuit,telefono,email,calle,numero)
CROSS APPLY (SELECT TOP 1 id_direccion FROM Direccion
             WHERE calle = v.calle AND numero = v.numero) d
WHERE NOT EXISTS (SELECT 1 FROM Inmobiliaria i WHERE i.cuit = v.cuit);
GO

--------------------------------------PERSONAS--------------------------------------

INSERT INTO Persona(nombre, apellido, dni, cuil, telefono, email, fecha_nacimiento, id_direccion)
SELECT v.nombre, v.apellido, v.dni, v.cuil, v.telefono, v.email, v.nacimiento, d.id_direccion
FROM (VALUES
 ('Camila','Ferreyra','31000001','27310000015','351-555-2000','camila.ferreyra@example.test',CONVERT(date,'1998-12-14'),'Río de Janeiro',560),
 ('Joaquín','Benítez','31000002','20310000016','351-555-2001','joaquin.benitez@example.test',CONVERT(date,'1976-06-22'),'Constitución',710),
 ('Martina','Sosa','31000003','27310000031','351-555-2002','martina.sosa@example.test',CONVERT(date,'1971-03-22'),'Av. Goycoechea',650),
 ('Bruno','Quiroga','31000004','20310000032','351-555-2003','bruno.quiroga@example.test',CONVERT(date,'1974-06-10'),'Av. San Martín',2210),
 ('Agustina','Molina','31000005','27310000047','351-555-2004','agustina.molina@example.test',CONVERT(date,'1994-01-07'),'Av. Vélez Sarsfield',2210),
 ('Nicolás','Ponce','31000006','20310000048','351-555-2005','nicolas.ponce@example.test',CONVERT(date,'1973-04-18'),'Los Tilos',970),
 ('Julieta','Ávila','31000007','27310000063','351-555-2006','julieta.avila@example.test',CONVERT(date,'1978-04-15'),'Constitución',3460),
 ('Federico','Luna','31000008','20310000064','351-555-2007','federico.luna@example.test',CONVERT(date,'2000-12-26'),'Av. Uruguay',3170),
 ('Rocío','Maldonado','32000001','27320000014','351-555-2008','rocio.maldonado@example.test',CONVERT(date,'1988-07-25'),'Sobremonte',1220),
 ('Santiago','Bustos','32000002','20320000015','351-555-2009','santiago.bustos@example.test',CONVERT(date,'1994-04-22'),'Av. San Martín',3340),
 ('Florencia','Cáceres','32000003','27320000030','351-555-2010','florencia.caceres@example.test',CONVERT(date,'1995-08-03'),'Río de Janeiro',560),
 ('Emiliano','Ortiz','32000004','20320000031','351-555-2011','emiliano.ortiz@example.test',CONVERT(date,'1988-05-23'),'Constitución',710),
 ('Delfina','Navarro','32000005','27320000046','351-555-2012','delfina.navarro@example.test',CONVERT(date,'1989-07-12'),'Av. Goycoechea',650),
 ('Ramiro','Vega','32000006','20320000047','351-555-2013','ramiro.vega@example.test',CONVERT(date,'1998-04-20'),'Av. San Martín',2210),
 ('Pilar','Figueroa','32000007','27320000062','351-555-2014','pilar.figueroa@example.test',CONVERT(date,'1987-08-02'),'Av. Vélez Sarsfield',2210),
 ('Ignacio','Peralta','32000008','20320000063','351-555-2015','ignacio.peralta@example.test',CONVERT(date,'1969-08-13'),'Los Tilos',970),
 ('Carolina','Juárez','33000001','27330000013','351-555-2016','carolina.juarez@example.test',CONVERT(date,'1972-05-12'),'Constitución',3460),
 ('Gonzalo','Arias','33000002','20330000014','351-555-2017','gonzalo.arias@example.test',CONVERT(date,'1998-03-24'),'Av. Uruguay',3170),
 ('Mariana','Castro','33000003','27330000029','351-555-2018','mariana.castro@example.test',CONVERT(date,'1975-05-21'),'Sobremonte',1220),
 ('Lucas','Moyano','33000004','20330000030','351-555-2019','lucas.moyano@example.test',CONVERT(date,'1975-07-25'),'Av. San Martín',3340)
) v(nombre,apellido,dni,cuil,telefono,email,nacimiento,calle,numero)
CROSS APPLY (SELECT TOP 1 id_direccion FROM Direccion
             WHERE calle = v.calle AND numero = v.numero) d
WHERE NOT EXISTS (SELECT 1 FROM Persona p WHERE p.dni = v.dni);
GO

/* Roles: una persona puede tener mas de uno (relacion N:M con TipoPersona) */
INSERT INTO PersonaTipo(id_persona, id_tipo_persona)
SELECT p.id_persona, tp.id_tipo_persona
FROM (VALUES
 ('31000001','Propietario'),
 ('31000002','Propietario'),
 ('31000003','Propietario'),
 ('31000004','Propietario'),
 ('31000005','Propietario'),
 ('31000006','Propietario'),
 ('31000007','Propietario'),
 ('31000008','Propietario'),
 ('32000001','Cliente'),
 ('32000002','Cliente'),
 ('32000003','Cliente'),
 ('32000004','Cliente'),
 ('32000005','Cliente'),
 ('32000006','Cliente'),
 ('32000007','Cliente'),
 ('32000008','Cliente'),
 ('33000001','Agente inmobiliario'),
 ('33000002','Agente inmobiliario'),
 ('33000003','Agente inmobiliario'),
 ('33000004','Agente inmobiliario')
) v(dni,rol)
JOIN Persona p ON p.dni = v.dni
JOIN TipoPersona tp ON tp.descripcion = v.rol
WHERE NOT EXISTS (SELECT 1 FROM PersonaTipo pt
                  WHERE pt.id_persona = p.id_persona
                    AND pt.id_tipo_persona = tp.id_tipo_persona);
GO

--------------------------------------INMUEBLES--------------------------------------

INSERT INTO Inmueble(codigo, titulo, descripcion, superficie_total, superficie_cubierta,
                     ambientes, dormitorios, banios, cochera,
                     id_direccion, id_tipo_inmueble, id_estado_inmueble, id_inmobiliaria)
SELECT v.codigo, v.titulo, v.descripcion, v.total, v.cubierta,
       v.amb, v.dorm, v.ban, v.cochera,
       d.id_direccion, ti.id_tipo_inmueble, ei.id_estado_inmueble, inm.id_inmobiliaria
FROM (VALUES
 ('COR-0006','Casa 4 dormitorios en Villa Carlos Paz','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',528.00,111.00,6,4,3,1,'Libertad',3190,'Casa','Patio Olmos Bienes Raíces'),
 ('COR-0007','Casa 3 dormitorios en La Falda','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',229.00,173.00,5,3,3,1,'Av. Edén',2290,'Casa','Altos del Suquía Propiedades'),
 ('COR-0008','Semipiso luminoso en Villa María','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',106.00,104.00,1,0,1,1,'Entre Ríos',3420,'Departamento','Sierras Chicas Inmobiliaria'),
 ('COR-0009','Casa en barrio cerrado en Villa María','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',524.00,259.00,5,4,1,1,'Entre Ríos',760,'Casa','Lago Azul Propiedades'),
 ('COR-0010','Semipiso luminoso en Mendiolaza','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',100.00,98.00,2,1,1,0,'Río Primero',4620,'Departamento','Patio Olmos Bienes Raíces'),
 ('COR-0011','Chalet familiar en Mendiolaza','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',338.00,200.00,4,3,3,0,'Río Primero',490,'Casa','Córdoba Hogar Inmobiliaria'),
 ('COR-0012','Lote esquina en Villa María','Lote con todos los servicios disponibles en el frente, apto para vivienda unifamiliar.',337.00,NULL,NULL,NULL,NULL,0,'Av. Sabattini',1370,'Terreno','Córdoba Hogar Inmobiliaria'),
 ('COR-0013','Monoambiente a estrenar en Alta Gracia','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',71.00,70.00,3,2,2,0,'Belgrano',1900,'Departamento','Córdoba Hogar Inmobiliaria'),
 ('COR-0014','Departamento con balcón en Río Cuarto','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',116.00,116.00,3,2,2,1,'Av. España',4350,'Departamento','Lago Azul Propiedades'),
 ('COR-0015','Departamento 2 ambientes en Córdoba','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',87.00,83.00,2,1,1,1,'Independencia',2940,'Departamento','Patio Olmos Bienes Raíces'),
 ('COR-0016','Departamento 2 ambientes en La Falda','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',65.00,65.00,2,1,1,1,'España',1850,'Departamento','Sierras Chicas Inmobiliaria'),
 ('COR-0017','Casa con patio y quincho en Unquillo','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',371.00,132.00,5,2,1,1,'Los Álamos',340,'Casa','Altos del Suquía Propiedades'),
 ('COR-0018','Departamento con balcón en Villa Carlos Paz','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',94.00,87.00,4,3,2,1,'Los Cedros',820,'Departamento','Sierras Chicas Inmobiliaria'),
 ('COR-0019','Salón comercial en Córdoba','Local sobre zona de alto tránsito peatonal, apto para múltiples rubros.',55.00,55.00,1,0,1,0,'Av. Hipólito Yrigoyen',4780,'Local comercial','Sierras Chicas Inmobiliaria'),
 ('COR-0020','Monoambiente a estrenar en Río Ceballos','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',107.00,102.00,4,3,2,1,'Los Tilos',2780,'Departamento','Córdoba Hogar Inmobiliaria'),
 ('COR-0021','Casa con patio y quincho en Villa María','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',493.00,205.00,3,3,1,1,'Av. Sabattini',3020,'Casa','Sierras Chicas Inmobiliaria'),
 ('COR-0022','Monoambiente a estrenar en La Falda','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',100.00,99.00,2,1,1,0,'España',2790,'Departamento','Córdoba Hogar Inmobiliaria'),
 ('COR-0023','Casa 4 dormitorios en Mendiolaza','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',419.00,112.00,5,4,3,1,'Río Primero',4170,'Casa','Altos del Suquía Propiedades'),
 ('COR-0024','Local comercial sobre avenida en Río Ceballos','Local sobre zona de alto tránsito peatonal, apto para múltiples rubros.',169.00,169.00,2,0,1,1,'Av. San Martín',2590,'Local comercial','Córdoba Hogar Inmobiliaria'),
 ('COR-0025','Monoambiente a estrenar en Río Cuarto','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',41.00,38.00,1,0,1,1,'Sobremonte',2530,'Departamento','Lago Azul Propiedades'),
 ('COR-0026','Departamento con balcón en Cosquín','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',38.00,32.00,2,1,1,0,'Tucumán',4030,'Departamento','Patio Olmos Bienes Raíces'),
 ('COR-0027','Departamento 3 ambientes en Mendiolaza','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',109.00,101.00,3,2,2,1,'Los Nogales',3090,'Departamento','Córdoba Hogar Inmobiliaria'),
 ('COR-0028','Monoambiente a estrenar en Río Ceballos','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',108.00,101.00,4,3,2,0,'Los Tilos',2690,'Departamento','Altos del Suquía Propiedades'),
 ('COR-0029','Terreno en loteo privado en Alta Gracia','Lote con todos los servicios disponibles en el frente, apto para vivienda unifamiliar.',614.00,NULL,NULL,NULL,NULL,0,'Av. Sarmiento',3390,'Terreno','Lago Azul Propiedades'),
 ('COR-0030','Lote esquina en Córdoba','Lote con todos los servicios disponibles en el frente, apto para vivienda unifamiliar.',658.00,NULL,NULL,NULL,NULL,0,'Montevideo',3530,'Terreno','Patio Olmos Bienes Raíces'),
 ('COR-0031','Lote apto construcción en Jesús María','Lote con todos los servicios disponibles en el frente, apto para vivienda unifamiliar.',854.00,NULL,NULL,NULL,NULL,0,'Tucumán',2890,'Terreno','Patio Olmos Bienes Raíces'),
 ('COR-0032','Departamento 4 ambientes en Río Cuarto','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',69.00,61.00,4,3,2,1,'Av. España',3750,'Departamento','Sierras Chicas Inmobiliaria'),
 ('COR-0033','Chalet familiar en La Falda','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',167.00,123.00,5,2,3,1,'España',630,'Casa','Córdoba Hogar Inmobiliaria'),
 ('COR-0034','Departamento 2 ambientes en Unquillo','Unidad en excelente estado, cercana a transporte público, comercios y servicios.',88.00,80.00,2,1,1,0,'Los Álamos',370,'Departamento','Sierras Chicas Inmobiliaria'),
 ('COR-0035','Chalet familiar en Córdoba','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',286.00,146.00,5,4,1,1,'Independencia',2910,'Casa','Altos del Suquía Propiedades'),
 ('COR-0036','Chalet familiar en La Falda','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',430.00,250.00,5,3,2,1,'España',910,'Casa','Lago Azul Propiedades'),
 ('COR-0037','Chalet familiar en La Falda','Propiedad en zona residencial tranquila, con buena orientación y espacios amplios.',360.00,100.00,4,3,1,1,'Av. Edén',160,'Casa','Sierras Chicas Inmobiliaria')
) v(codigo,titulo,descripcion,total,cubierta,amb,dorm,ban,cochera,calle,numero,tipo,inmobiliaria)
CROSS APPLY (SELECT TOP 1 id_direccion FROM Direccion
             WHERE calle = v.calle AND numero = v.numero) d
JOIN TipoInmueble ti ON ti.descripcion = v.tipo
JOIN Inmobiliaria inm ON inm.nombre = v.inmobiliaria
/* Todo inmueble nace Disponible: el estado final lo definen los contratos */
JOIN EstadoInmueble ei ON ei.descripcion = 'Disponible'
WHERE NOT EXISTS (SELECT 1 FROM Inmueble i WHERE i.codigo = v.codigo);
GO

--------------------------------------PROPIETARIOS--------------------------------------

/* Relacion N:M con porcentaje. El trigger tr_PropietarioInmueble_Porcentaje
   impide que la suma por inmueble supere el 100%. */
INSERT INTO PropietarioInmueble(id_persona, id_inmueble, porcentaje_propiedad)
SELECT p.id_persona, i.id_inmueble, v.porcentaje
FROM (VALUES
 ('31000001','COR-0006',50.00),
 ('31000004','COR-0006',50.00),
 ('31000002','COR-0007',100.00),
 ('31000003','COR-0008',100.00),
 ('31000004','COR-0009',100.00),
 ('31000005','COR-0010',100.00),
 ('31000006','COR-0011',50.00),
 ('31000001','COR-0011',50.00),
 ('31000007','COR-0012',100.00),
 ('31000008','COR-0013',100.00),
 ('31000001','COR-0014',100.00),
 ('31000002','COR-0015',50.00),
 ('31000005','COR-0015',50.00),
 ('31000003','COR-0016',100.00),
 ('31000004','COR-0017',50.00),
 ('31000007','COR-0017',50.00),
 ('31000005','COR-0018',100.00),
 ('31000006','COR-0019',100.00),
 ('31000007','COR-0020',100.00),
 ('31000008','COR-0021',50.00),
 ('31000003','COR-0021',50.00),
 ('31000001','COR-0022',50.00),
 ('31000004','COR-0022',50.00),
 ('31000002','COR-0023',100.00),
 ('31000003','COR-0024',100.00),
 ('31000004','COR-0025',100.00),
 ('31000005','COR-0026',100.00),
 ('31000006','COR-0027',50.00),
 ('31000001','COR-0027',50.00),
 ('31000007','COR-0028',100.00),
 ('31000008','COR-0029',50.00),
 ('31000003','COR-0029',50.00),
 ('31000001','COR-0030',100.00),
 ('31000002','COR-0031',100.00),
 ('31000003','COR-0032',50.00),
 ('31000006','COR-0032',50.00),
 ('31000004','COR-0033',100.00),
 ('31000005','COR-0034',100.00),
 ('31000006','COR-0035',100.00),
 ('31000007','COR-0036',100.00),
 ('31000008','COR-0037',100.00)
) v(dni,codigo,porcentaje)
JOIN Persona p ON p.dni = v.dni
JOIN Inmueble i ON i.codigo = v.codigo
WHERE NOT EXISTS (SELECT 1 FROM PropietarioInmueble pi
                  WHERE pi.id_persona = p.id_persona AND pi.id_inmueble = i.id_inmueble);
GO

--------------------------------------PUBLICACIONES--------------------------------------

/* Fechas escalonadas durante el ultimo anio, para que los reportes con filtro
   por periodo devuelvan subconjuntos distintos y no todo junto. */
INSERT INTO Publicacion(id_inmueble, id_tipo_operacion, precio, moneda, fecha_publicacion, activa)
SELECT i.id_inmueble, op.id_tipo_operacion, v.precio, v.moneda, v.fecha, 1
FROM (VALUES
 ('COR-0006','Venta',433000000.00,'ARS',CONVERT(date,'2026-01-24')),
 ('COR-0007','Venta',188000000.00,'ARS',CONVERT(date,'2026-06-06')),
 ('COR-0008','Venta',122000.00,'USD',CONVERT(date,'2026-05-14')),
 ('COR-0009','Venta',409000.00,'USD',CONVERT(date,'2026-07-24')),
 ('COR-0010','Venta',115000.00,'USD',CONVERT(date,'2026-03-07')),
 ('COR-0011','Venta',264000.00,'USD',CONVERT(date,'2025-12-22')),
 ('COR-0012','Venta',54000.00,'USD',CONVERT(date,'2026-09-15')),
 ('COR-0013','Alquiler',460000.00,'ARS',CONVERT(date,'2026-09-12')),
 ('COR-0014','Alquiler',750000.00,'ARS',CONVERT(date,'2026-09-23')),
 ('COR-0015','Venta',109000000.00,'ARS',CONVERT(date,'2026-07-17')),
 ('COR-0016','Venta',75000.00,'USD',CONVERT(date,'2026-09-05')),
 ('COR-0017','Venta',289000.00,'USD',CONVERT(date,'2026-07-10')),
 ('COR-0018','Alquiler',610000.00,'ARS',CONVERT(date,'2026-10-01')),
 ('COR-0019','Venta',77000000.00,'ARS',CONVERT(date,'2026-02-14')),
 ('COR-0020','Venta',123000.00,'USD',CONVERT(date,'2025-12-23')),
 ('COR-0021','Venta',404000000.00,'ARS',CONVERT(date,'2026-01-06')),
 ('COR-0022','Alquiler',650000.00,'ARS',CONVERT(date,'2026-06-08')),
 ('COR-0023','Venta',327000.00,'USD',CONVERT(date,'2026-03-25')),
 ('COR-0024','Alquiler',1520000.00,'ARS',CONVERT(date,'2026-06-01')),
 ('COR-0025','Alquiler',270000.00,'ARS',CONVERT(date,'2026-10-19')),
 ('COR-0026','Alquiler',250000.00,'ARS',CONVERT(date,'2026-04-15')),
 ('COR-0027','Alquiler',710000.00,'ARS',CONVERT(date,'2026-04-11')),
 ('COR-0028','Venta',124000.00,'USD',CONVERT(date,'2026-09-29')),
 ('COR-0029','Venta',111000000.00,'ARS',CONVERT(date,'2026-09-26')),
 ('COR-0030','Venta',118000000.00,'ARS',CONVERT(date,'2026-09-20')),
 ('COR-0031','Venta',137000.00,'USD',CONVERT(date,'2026-01-15')),
 ('COR-0032','Alquiler',450000.00,'ARS',CONVERT(date,'2026-06-07')),
 ('COR-0033','Venta',137000000.00,'ARS',CONVERT(date,'2026-09-25')),
 ('COR-0034','Venta',110000000.00,'ARS',CONVERT(date,'2026-05-04')),
 ('COR-0035','Venta',223000.00,'USD',CONVERT(date,'2026-01-25')),
 ('COR-0036','Venta',335000.00,'USD',CONVERT(date,'2026-03-10')),
 ('COR-0037','Venta',281000.00,'USD',CONVERT(date,'2026-07-03'))
) v(codigo,operacion,precio,moneda,fecha)
JOIN Inmueble i ON i.codigo = v.codigo
JOIN TipoOperacion op ON op.descripcion = v.operacion
/* El guard mira si el inmueble ya tiene CUALQUIER publicacion, activa o no:
   si mirara solo las activas, al volver a correr el script se crearia una
   publicacion nueva para un inmueble que ya esta vendido. */
WHERE NOT EXISTS (SELECT 1 FROM Publicacion pu WHERE pu.id_inmueble = i.id_inmueble);
GO

--------------------------------------CONTRATOS--------------------------------------

/* Al insertarse con estado Vigente, los triggers dan de baja la publicacion
   y pasan el inmueble a Vendido o Alquilado segun el tipo de operacion. */
INSERT INTO Contrato(id_publicacion, id_cliente, id_agente, fecha_inicio, fecha_fin,
                     monto_acordado, comision_porcentaje, estado)
SELECT pu.id_publicacion, cli.id_persona, ag.id_persona, v.inicio, v.fin,
       v.monto, v.comision, 'Vigente'
FROM (VALUES
 ('COR-0009','32000001','33000001',CONVERT(date,'2026-08-13'),NULL,409000.00,2.86),
 ('COR-0015','32000002','33000002',CONVERT(date,'2026-08-22'),NULL,109000000.00,3.32),
 ('COR-0024','32000003','33000003',CONVERT(date,'2026-06-22'),CONVERT(date,'2028-06-21'),1520000.00,8.92),
 ('COR-0006','32000004','33000004',CONVERT(date,'2026-03-09'),NULL,433000000.00,3.62),
 ('COR-0031','32000005','33000001',CONVERT(date,'2026-03-09'),NULL,137000.00,2.94),
 ('COR-0023','32000006','33000002',CONVERT(date,'2026-04-25'),NULL,327000.00,3.00),
 ('COR-0017','32000007','33000003',CONVERT(date,'2026-08-27'),NULL,289000.00,2.85),
 ('COR-0022','32000008','33000004',CONVERT(date,'2026-07-16'),CONVERT(date,'2028-07-15'),650000.00,6.06),
 ('COR-0010','32000001','33000001',CONVERT(date,'2026-04-25'),NULL,115000.00,3.48),
 ('COR-0027','32000002','33000002',CONVERT(date,'2026-06-01'),CONVERT(date,'2028-05-31'),710000.00,7.42),
 ('COR-0035','32000003','33000003',CONVERT(date,'2026-03-11'),NULL,223000.00,2.55)
) v(codigo,dni_cliente,dni_agente,inicio,fin,monto,comision)
JOIN Inmueble i ON i.codigo = v.codigo
JOIN Publicacion pu ON pu.id_inmueble = i.id_inmueble
JOIN Persona cli ON cli.dni = v.dni_cliente
JOIN Persona ag ON ag.dni = v.dni_agente
WHERE NOT EXISTS (SELECT 1 FROM Contrato co WHERE co.id_publicacion = pu.id_publicacion);
GO

--------------------------------------FACTURAS--------------------------------------

/* Se insertan directo, y no con sp_EmitirFactura, porque ese procedimiento
   siempre usa GETDATE() y todas quedarian emitidas hoy: sin fechas distintas
   el reporte de facturacion por periodo no tendria nada que filtrar.
   El procedimiento queda libre para demostrarlo en vivo. */
INSERT INTO Factura(numero_factura, id_contrato, fecha_emision, fecha_vencimiento, estado)
SELECT v.numero, co.id_contrato, v.emision, v.vencimiento, v.estado
FROM (VALUES
 ('FC-A-2026-0001','COR-0009',CONVERT(date,'2026-08-23'),CONVERT(date,'2026-09-22'),'Pagada'),
 ('FC-A-2026-0002','COR-0015',CONVERT(date,'2026-08-28'),CONVERT(date,'2026-09-27'),'Pagada'),
 ('FC-A-2026-0003','COR-0024',CONVERT(date,'2026-06-28'),CONVERT(date,'2026-07-28'),'Pagada'),
 ('FC-A-2026-0004','COR-0006',CONVERT(date,'2026-03-14'),CONVERT(date,'2026-04-13'),'Pagada'),
 ('FC-A-2026-0005','COR-0031',CONVERT(date,'2026-03-13'),CONVERT(date,'2026-04-12'),'Pagada'),
 ('FC-A-2026-0006','COR-0023',CONVERT(date,'2026-05-05'),CONVERT(date,'2026-06-04'),'Pendiente'),
 ('FC-A-2026-0007','COR-0017',CONVERT(date,'2026-09-03'),CONVERT(date,'2026-10-03'),'Pagada'),
 ('FC-A-2026-0008','COR-0022',CONVERT(date,'2026-07-17'),CONVERT(date,'2026-08-16'),'Pendiente'),
 ('FC-A-2026-0009','COR-0010',CONVERT(date,'2026-04-28'),CONVERT(date,'2026-05-28'),'Pagada'),
 ('FC-A-2026-0010','COR-0027',CONVERT(date,'2026-06-05'),CONVERT(date,'2026-07-05'),'Pagada'),
 ('FC-A-2026-0011','COR-0035',CONVERT(date,'2026-03-13'),CONVERT(date,'2026-04-12'),'Pagada')
) v(numero,codigo,emision,vencimiento,estado)
JOIN Inmueble i ON i.codigo = v.codigo
JOIN Publicacion pu ON pu.id_inmueble = i.id_inmueble
JOIN Contrato co ON co.id_publicacion = pu.id_publicacion
WHERE NOT EXISTS (SELECT 1 FROM Factura f WHERE f.numero_factura = v.numero);
GO

/* El trigger tr_DetalleFactura_CalcularSubtotal recalcula el subtotal,
   asi que alcanza con mandar cantidad y precio unitario. */
INSERT INTO DetalleFactura(id_factura, concepto, cantidad, precio_unitario, subtotal)
SELECT f.id_factura, v.concepto, 1, v.precio, v.precio
FROM (VALUES
 ('FC-A-2026-0001','Comisión por venta - COR-0009',11697.40),
 ('FC-A-2026-0002','Comisión por venta - COR-0015',3618800.00),
 ('FC-A-2026-0003','Comisión por alquiler - COR-0024',135584.00),
 ('FC-A-2026-0004','Comisión por venta - COR-0006',15674600.00),
 ('FC-A-2026-0005','Comisión por venta - COR-0031',4027.80),
 ('FC-A-2026-0006','Comisión por venta - COR-0023',9810.00),
 ('FC-A-2026-0007','Comisión por venta - COR-0017',8236.50),
 ('FC-A-2026-0008','Comisión por alquiler - COR-0022',39390.00),
 ('FC-A-2026-0009','Comisión por venta - COR-0010',4002.00),
 ('FC-A-2026-0010','Comisión por alquiler - COR-0027',52682.00),
 ('FC-A-2026-0011','Comisión por venta - COR-0035',5686.50)
) v(numero,concepto,precio)
JOIN Factura f ON f.numero_factura = v.numero
WHERE NOT EXISTS (SELECT 1 FROM DetalleFactura df WHERE df.id_factura = f.id_factura);
GO

--------------------------------------VERIFICACION--------------------------------------

/* Control rapido: ninguna de estas filas deberia dar cero. */
SELECT 'Inmobiliarias' AS tabla, COUNT(*) AS filas FROM Inmobiliaria
UNION ALL SELECT 'Personas', COUNT(*) FROM Persona
UNION ALL SELECT 'Inmuebles', COUNT(*) FROM Inmueble
UNION ALL SELECT 'Publicaciones', COUNT(*) FROM Publicacion
UNION ALL SELECT 'Publicaciones activas', COUNT(*) FROM Publicacion WHERE activa = 1
UNION ALL SELECT 'Contratos', COUNT(*) FROM Contrato
UNION ALL SELECT 'Facturas', COUNT(*) FROM Factura
UNION ALL SELECT 'Detalles de factura', COUNT(*) FROM DetalleFactura;
GO

/* Como quedaron los inmuebles despues de que actuaron los triggers */
SELECT ei.descripcion AS estado, COUNT(*) AS cantidad
FROM Inmueble i
JOIN EstadoInmueble ei ON ei.id_estado_inmueble = i.id_estado_inmueble
GROUP BY ei.descripcion
ORDER BY cantidad DESC;
GO
