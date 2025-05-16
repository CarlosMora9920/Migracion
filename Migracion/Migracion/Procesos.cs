using Migracion.Clases;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Migracion
{
     class Procesos
    {

        public static void InsertarVendedores(List<t_vended> vendedList)
        {
            using (var conn = new NpgsqlConnection("Host=10.141.10.10:9088;Username=postgres;Password=#756913%;Database=mercacentro"))
            {
                conn.Open();

                // Iniciar transacción
                using (var tx = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tx;

                    // Insertar cada registro de vended
                    foreach (var vended in vendedList)
                    {
                        // Comando SQL con parámetros
                        cmd.CommandText = @"
                        INSERT INTO vendedores (codigo, nombre, telefonos, direccion, ciudad, numerodedocumento, fechahoraingreso, estado, sucursalid)
                        VALUES (@codigo, @nombre, @telefonos, @direccion, @ciudad, @numerodedocumento, @fechahoraingreso, @estado, @sucursalid)";

                        // Parametrización de la consulta
                        cmd.Parameters.Clear();  // Limpiar los parámetros antes de agregar nuevos
                        cmd.Parameters.AddWithValue("codigo", vended.Vended.Trim());
                        cmd.Parameters.AddWithValue("nombre", vended.Nombre.Trim());
                        cmd.Parameters.AddWithValue("telefonos", vended.Tel.Trim());
                        cmd.Parameters.AddWithValue("direccion", vended.Direcc.Trim());
                        cmd.Parameters.AddWithValue("ciudad", vended.Ciudad.Trim());
                        cmd.Parameters.AddWithValue("numerodedocumento", vended.Cedula);
                        cmd.Parameters.AddWithValue("fechahoraingreso", vended.FechIng); // Asegúrate de que la fecha sea del tipo correcto
                        cmd.Parameters.AddWithValue("estado", true); // Por ejemplo, siempre `true` para estado
                        cmd.Parameters.AddWithValue("sucursalid", 10); // Un valor constante para sucursalid (puedes ajustarlo)

                        // Ejecutar el comando de inserción
                        cmd.ExecuteNonQuery();
                    }

                    // Confirmar la transacción
                    tx.Commit();
                }
            }

            Console.WriteLine("Datos insertados correctamente.");
        }

        public static void InsertarClientes(List<t_cliente> Clien_list, List<t_ciudad> ciudade_lis)
        {

            int xTipoPersona = 0;
            string xDepartamento = "";
            string xMunicipio = "";

            int contador = 0; // Contador

            using (var conn = new NpgsqlConnection("Host=10.141.10.10:9088;Username=postgres;Password=#756913%;Database=mercacentro"))
            {
                conn.Open();

                // Iniciar transacción
                using (var tx = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tx;

                    foreach (var clientes in Clien_list)
                    {
                        try
                        {
                            cmd.Parameters.Clear(); // ← CRUCIA

                            // Comando SQL con parámetros
                            cmd.CommandText = @"
                       INSERT INTO clientes (
                        idtiposdepersona, idtiposdedocumento, numerodedocumento, digitodeverificacion, 
                        razonsocial, direccion, emails, telefonos, idmunicipios, papellido, 
                        sapellido, pnombre, snombre, estado, iddepartamento
                        )
                        VALUES (
                        @tipoPersona,
                        (SELECT id FROM tiposdedocumento WHERE codigo = @codigoDoc LIMIT 1),
                        @numeroDoc,
                        @dv,
                        @razonSocial,
                        @direccion,
                        @email,
                        @telefono,
                        (SELECT id FROM municipios WHERE nombre = @municipio LIMIT 1),
                        @apl1,
                        @apl2,
                        @nom1,
                        @nom2,
                        @estado,
                        (SELECT id FROM departamentos WHERE nombre = @departamento LIMIT 1)
                        );";

                            if (clientes.tipo_per == "Natural")
                            {
                                xTipoPersona = 1;
                            }
                            else
                            {
                                xTipoPersona = 2;
                            }


                            var ubicacion = ciudade_lis.FirstOrDefault(c => c.Dane == clientes.Dane);


                            if (ubicacion != null)
                            {
                                xDepartamento = ubicacion.Departamento;
                                xMunicipio = ubicacion.Municipio;
                            }

                            cmd.Parameters.AddWithValue("tipoPersona", xTipoPersona);
                            cmd.Parameters.AddWithValue("codigoDoc", string.IsNullOrWhiteSpace(clientes.tdoc) ? DBNull.Value : clientes.tdoc);
                            cmd.Parameters.AddWithValue("numeroDoc", BigInteger.Parse(clientes.anexo));
                            cmd.Parameters.AddWithValue("dv", string.IsNullOrWhiteSpace(clientes.dv) ? DBNull.Value : BigInteger.Parse(clientes.dv));
                            cmd.Parameters.AddWithValue("razonSocial", string.IsNullOrWhiteSpace(clientes.nombre) ? DBNull.Value : clientes.nombre);
                            cmd.Parameters.AddWithValue("direccion", string.IsNullOrWhiteSpace(clientes.direcc) ? DBNull.Value : clientes.direcc);
                            cmd.Parameters.AddWithValue("email", string.IsNullOrWhiteSpace(clientes.emailfe1) ? DBNull.Value : clientes.emailfe1);
                            cmd.Parameters.AddWithValue("telefono", string.IsNullOrWhiteSpace(clientes.tel) ? DBNull.Value : clientes.tel);
                            cmd.Parameters.AddWithValue("municipio", string.IsNullOrWhiteSpace(xMunicipio) ? DBNull.Value : xMunicipio);
                            cmd.Parameters.AddWithValue("apl1", string.IsNullOrWhiteSpace(clientes.apl1) ? DBNull.Value : clientes.apl1);
                            cmd.Parameters.AddWithValue("apl2", string.IsNullOrWhiteSpace(clientes.apl2) ? DBNull.Value : clientes.apl2);
                            cmd.Parameters.AddWithValue("nom1", string.IsNullOrWhiteSpace(clientes.nom1) ? DBNull.Value : clientes.nom1);
                            cmd.Parameters.AddWithValue("nom2", string.IsNullOrWhiteSpace(clientes.nom2) ? DBNull.Value : clientes.nom2);
                            cmd.Parameters.AddWithValue("estado", clientes.bloqueado == 1 ? false : true);
                            cmd.Parameters.AddWithValue("departamento", string.IsNullOrWhiteSpace(xDepartamento) ? DBNull.Value : xDepartamento);

                            // Ejecutar el comando de inserción
                            cmd.ExecuteNonQuery();



                            contador++; // Incrementar contador
                            Console.WriteLine($"Cliente insertado #{contador} - Documento: {clientes.anexo}"); // Mostrar en consola
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error al insertar cliente con documento {clientes.anexo}: {ex.Message}");
                        }
                    }

                    // Confirmar la transacción
                    tx.Commit();
                }
            }

            Console.WriteLine("Datos insertados correctamente.");

        }

        public static void InsertarHistoNove(List<t_histonove> nove_list)
        {

            int contador = 0; // Contador

            using (var conn = new NpgsqlConnection("Host=10.141.10.10:9088;Username=postgres;Password=#756913%;Database=mercacentro"))
            {
                conn.Open();

                // Iniciar transacción
                using (var tx = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tx;

                    foreach (var histonove in nove_list)
                    {
                        try
                        {
                            cmd.Parameters.Clear(); // ← CRUCIA

                            // Comando SQL con parámetros
                            cmd.CommandText = @"
                        INSERT INTO cambioeps (
                            entidadesnominaanteriorid,
                            entidadesnominanuevaid,
                            usuarioid,
                            fechahora,
                            empleadoid,
                            reportartraslado,
                            fechacambio
                        )
                        VALUES (
                            (SELECT DISTINCT id FROM public.""EntidadesNomina"" WHERE codigo = @codAnt),
                            (SELECT DISTINCT id FROM public.""EntidadesNomina""WHERE codigo = @codAct),
                            (SELECT DISTINCT  ""Id"" FROM public.""AspNetUsers"" WHERE ""UserName"" = @userReg),
                            @fechaHora,
                            (SELECT DISTINCT ""Id"" FROM public.""Empleados"" WHERE ""CodigoEmpleado"" = @empleado),
                            @reportarTraslado,
                            @fechaCambio
                        );";


                            cmd.Parameters.AddWithValue("codAnt", histonove.cod_ant.Trim());
                            cmd.Parameters.AddWithValue("codAct", histonove.cod_act.Trim());
                            cmd.Parameters.AddWithValue("userReg", histonove.usua_reg.Trim());
                            cmd.Parameters.AddWithValue("fechaHora", new DateTime(
                                histonove.fech_reg.Year,
                                histonove.fech_reg.Month,
                                histonove.fech_reg.Day,
                                histonove.hora_reg.Hour,
                                histonove.hora_reg.Minute,
                                histonove.hora_reg.Second
                            ));
                            cmd.Parameters.AddWithValue("empleado", histonove.empleado.Trim());
                            cmd.Parameters.AddWithValue("reportarTraslado", histonove.repo_soi == 1);
                            cmd.Parameters.AddWithValue("fechaCambio", histonove.fech_camb);

                            // Ejecutar el comando de inserción
                            cmd.ExecuteNonQuery();



                            contador++; // Incrementar contador
                            Console.WriteLine($"Ingresando registro.. #{contador}"); // Mostrar en consola
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error al insertar cliente con documento: {ex.Message}");
                        }
                    }

                    // Confirmar la transacción
                    tx.Commit();
                }
            }

            Console.WriteLine("Datos insertados correctamente.");

        }

        public static void InsertarHistonoveP(List<t_histonove> nove_pen)
        {

            int contador = 0; // Contador

            using (var conn = new NpgsqlConnection("Host=10.141.10.10:9088;Username=postgres;Password=#756913%;Database=mercacentro"))
            {
                conn.Open();

                // Iniciar transacción
                using (var tx = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tx;

                    foreach (var histonove in nove_pen)
                    {
                        try
                        {
                            cmd.Parameters.Clear(); // ← CRUCIA

                            // Comando SQL con parámetros
                            cmd.CommandText = @"
                        INSERT INTO cambiopension (
                            entidadesnominaanteriorid,
                            entidadesnominanuevaid,
                            usuarioid,
                            fechahora,
                            empleadoid,
                            reportartraslado,
                            fechacambio
                        )
                        VALUES (
                            (SELECT DISTINCT id FROM public.""EntidadesNomina"" WHERE codigo = @codAnt),
                            (SELECT DISTINCT id FROM public.""EntidadesNomina""WHERE codigo = @codAct),
                            (SELECT DISTINCT  ""Id"" FROM public.""AspNetUsers"" WHERE ""UserName"" = @userReg),
                            @fechaHora,
                            (SELECT DISTINCT ""Id"" FROM public.""Empleados"" WHERE ""CodigoEmpleado"" = @empleado),
                            @reportarTraslado,
                            @fechaCambio
                        );";


                            cmd.Parameters.AddWithValue("codAnt", histonove.cod_ant.Trim());
                            cmd.Parameters.AddWithValue("codAct", histonove.cod_act.Trim());
                            cmd.Parameters.AddWithValue("userReg", histonove.usua_reg.Trim());
                            cmd.Parameters.AddWithValue("fechaHora", new DateTime(
                                histonove.fech_reg.Year,
                                histonove.fech_reg.Month,
                                histonove.fech_reg.Day,
                                histonove.hora_reg.Hour,
                                histonove.hora_reg.Minute,
                                histonove.hora_reg.Second
                            ));
                            cmd.Parameters.AddWithValue("empleado", histonove.empleado.Trim());
                            cmd.Parameters.AddWithValue("reportarTraslado", histonove.repo_soi == 1);
                            cmd.Parameters.AddWithValue("fechaCambio", histonove.fech_camb);

                            // Ejecutar el comando de inserción
                            cmd.ExecuteNonQuery();



                            contador++; // Incrementar contador
                            Console.WriteLine($"Cliente insertado #{contador}"); // Mostrar en consola
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error al insertar cliente con documento: {ex.Message}");
                        }
                    }

                    // Confirmar la transacción
                    tx.Commit();
                }
            }

            Console.WriteLine("Datos insertados correctamente.");

        }

        public static void InsertarHistonoveS(List<t_histonove> nove_pen)
        {
            int contador = 0; // Contador

            using (var conn = new NpgsqlConnection("Host=10.141.10.10:9088;Username=postgres;Password=#756913%;Database=mercacentro"))
            {
                conn.Open();

                // Iniciar transacción
                using (var tx = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tx;

                    foreach (var histonove in nove_pen)
                    {
                        try
                        {
                            cmd.Parameters.Clear(); // ← CRUCIA

                            // Comando SQL con parámetros
                            cmd.CommandText = @"
                        INSERT INTO cambiosalario (
                            salarioanterior,
                            salarionuevo,
                            usuarioid,
                            fechahora,
                            empleadoid,
                            reportavariacion,
                            fechahoravariacion
                        )
                        VALUES (
                            @val_ant,
                            @val_act,
                            (SELECT DISTINCT  ""Id"" FROM public.""AspNetUsers"" WHERE ""UserName"" = @userReg),
                            @fechaHora,
                            (SELECT DISTINCT ""Id"" FROM public.""Empleados"" WHERE ""CodigoEmpleado"" = @empleado),
                            @reportarTraslado,
                            @fechaCambio
                        );";


                            cmd.Parameters.AddWithValue("val_ant", histonove.val_ant);
                            cmd.Parameters.AddWithValue("val_act", histonove.val_act);
                            cmd.Parameters.AddWithValue("userReg", histonove.usua_reg.Trim());
                            cmd.Parameters.AddWithValue("fechaHora", new DateTime(
                                histonove.fech_reg.Year,
                                histonove.fech_reg.Month,
                                histonove.fech_reg.Day,
                                histonove.hora_reg.Hour,
                                histonove.hora_reg.Minute,
                                histonove.hora_reg.Second
                            ));
                            cmd.Parameters.AddWithValue("empleado", histonove.empleado.Trim());
                            cmd.Parameters.AddWithValue("reportarTraslado", histonove.repo_soi == 1);
                            cmd.Parameters.AddWithValue("fechaCambio", histonove.fech_camb);

                            // Ejecutar el comando de inserción
                            cmd.ExecuteNonQuery();



                            contador++; // Incrementar contador
                            Console.WriteLine($"Cliente insertado #{contador}"); // Mostrar en consola
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error al insertar cliente con documento: {ex.Message}");
                            //ex.InnerException
                        }
                    }

                    // Confirmar la transacción
                    tx.Commit();
                }
            }

            Console.WriteLine("Datos insertados correctamente.");

        }

        public static double CalcularRentabilidad(
        double pvtaxx, double pcosto, int pmodo, double piva,
        double pconsumo, int picon_1111,
        int pr_tipoiup, double pr_vr_ibu, DateTime pr_fech_iu, int pr_gen_iup)
        {
            if (double.IsNaN(picon_1111)) // FoxPro hace TYPE, aquí usamos NaN como equivalente inválido
                picon_1111 = 0;

            double x_porc = 0.00;

            if (pvtaxx == 0 || pcosto == 0)
                return 0;

            double xFact_iup = 0.00;
            double x_vr_ibu = 0.00;

            // Determinar impuesto saludable
            switch (pr_tipoiup)
            {
                case 1:
                    xFact_iup = FPorcIcup(pr_fech_iu);
                    break;

                case 2:
                    // Bebidas: no se hace nada según lógica actual
                    break;
            }

            double x_iva = 0;

            switch (pmodo)
            {
                case 1:
                    if (pcosto > 0)
                    {
                        x_iva = 1 + (piva / 100.0);
                        x_porc = Math.Round(((pvtaxx / ((pcosto * x_iva) + pconsumo)) - 1) * 100, 2);
                    }
                    break;

                case 2:
                    x_iva = 1 + (piva / 100.0);
                    x_porc = Math.Round((1 - ((pcosto + pconsumo) / (pvtaxx / x_iva))) * 100, 2);
                    break;

                case 3:
                    x_iva = 1 + ((piva + xFact_iup) / 100.0);

                    if (picon_1111 == 1)
                    {
                        x_porc = pvtaxx / (((pcosto + pr_vr_ibu) * x_iva) + pconsumo);
                        x_porc = (x_porc - 1) * 100;
                        x_porc = Math.Round(x_porc, 2);
                    }
                    else
                    {
                        x_porc = Math.Round(((pvtaxx / ((pcosto + pr_vr_ibu) * x_iva)) - 1) * 100.0, 2);
                    }
                    break;

                case 4:
                    x_iva = 1 + ((piva + xFact_iup) / 100.0);

                    if (picon_1111 == 1)
                    {
                        x_porc = pvtaxx / ((pcosto * x_iva) + pconsumo + pr_vr_ibu);
                        x_porc = (x_porc - 1) * 100;
                        x_porc = Math.Round(x_porc, 2);
                    }
                    else
                    {
                        x_porc = (pvtaxx - pconsumo) / ((pcosto * x_iva) + pr_vr_ibu);
                        x_porc = (x_porc - 1) * 100;
                        x_porc = Math.Round(x_porc, 2);
                    }
                    break;
            }

            if (x_porc > 999.99)
                x_porc = 999.99;

            if (x_porc < 0)
                x_porc = 0; // FoxPro no hace nada, puedes decidir si lo dejas en cero

            return x_porc;

        }


        private static double FPorcIcup(DateTime fecha)
        {
            // Simulación. Sustituir con la lógica real de cálculo del impuesto por fecha.
            int año = fecha.Year;
            if (año >= 2024) return 10.0;
            if (año == 2023) return 8.0;
            return 5.0;
        }


        public static decimal FCalcImp(
    decimal pVrBase,
    decimal pFiva,
    decimal pVrIconsu,
    int pModo,
    int pRound,
    int pmTipoIup,
    decimal pmVrIbu,
    DateTime pmFechIu,
    int pmGenIup,
    string pReturnUp)
        {
            int xNumRound = pRound;
            decimal xFactIup = 0.00m;
            decimal xVrIbu = 0.00m;

            if (pmGenIup == 1)
            {
                switch (pmTipoIup)
                {
                    case 1: // Comestibles
                        xFactIup = FporcIcup(pmFechIu);
                        break;
                    case 2: // Bebidas
                        xVrIbu = pmVrIbu;
                        break;
                }
            }

            decimal xVrBase = 0.00m;
            decimal xRetorno = 0.00m;
            decimal xBaseGrav = 0.00m;
            decimal xCalcIva = 0.00m;
            decimal xCalcUp = 0.00m;

            switch (pModo)
            {
                case 1: // Valor base con impuestos incluidos
                    xVrBase = pVrBase - pVrIconsu - xVrIbu;

                    decimal divisor = Math.Round(1 + Math.Round((pFiva + xFactIup) / 100.00m, 4), 4);
                    xBaseGrav = Math.Round(xVrBase / divisor, xNumRound);

                    if (pFiva > 0 && xFactIup == 0)
                    {
                        xCalcIva = xVrBase - xBaseGrav;
                        xCalcUp = pmTipoIup == 2 ? xVrIbu : 0;
                    }
                    else if (pFiva == 0 && xFactIup > 0)
                    {
                        xCalcIva = 0;
                        xCalcUp = xVrBase - xBaseGrav;
                    }
                    else if (pFiva > 0 && xFactIup > 0)
                    {
                        xCalcUp = Math.Round(xBaseGrav * Math.Round(xFactIup / 100.00m, 4), 2);
                        xCalcIva = xVrBase - xBaseGrav - xCalcUp;
                    }
                    else
                    {
                        xCalcIva = 0;
                        xCalcUp = pmTipoIup == 2 ? xVrIbu : 0;
                    }
                    break;

                case 2: // Valor base sin IVA ni ultraprocesados, pero con impoconsumo
                    xVrBase = pVrBase - pVrIconsu;

                    if (pFiva > 0 && xFactIup == 0)
                    {
                        xCalcIva = Math.Round(xVrBase * Math.Round(pFiva / 100.00m, 4), xNumRound);
                        xCalcUp = pmTipoIup == 2 ? xVrIbu : 0;
                    }
                    else if (pFiva == 0 && xFactIup > 0)
                    {
                        xCalcIva = 0;
                        xCalcUp = Math.Round(xVrBase * Math.Round(xFactIup / 100.00m, 4), xNumRound);
                    }
                    else if (pFiva > 0 && xFactIup > 0)
                    {
                        xCalcUp = Math.Round(xVrBase * Math.Round(xFactIup / 100.00m, 4), 2);
                        xCalcIva = Math.Round(xVrBase * Math.Round(pFiva / 100.00m, 4), 2);
                    }
                    else
                    {
                        xCalcIva = 0;
                        xCalcUp = pmTipoIup == 2 ? xVrIbu : 0;
                    }
                    break;
            }

            switch (pReturnUp)
            {
                case "U":
                    xRetorno = xCalcUp;
                    break;
                case "I":
                    xRetorno = xCalcIva;
                    break;
                case "A":
                    xRetorno = xCalcIva + xCalcUp;
                    break;
            }

            return xRetorno;
        }


        public static decimal FporcIcup(DateTime pFechCaus)
        {
            decimal xIcup = 0.00m;

            if (pFechCaus.Year == 2023)
            {
                DateTime xFeIcup = new DateTime(2023, 11, 1);

                if (pFechCaus >= xFeIcup)
                {
                    xIcup = 10.00m;
                }
            }
            else if (pFechCaus.Year == 2024)
            {
                xIcup = 15.00m;
            }
            else if (pFechCaus.Year == 2025)
            {
                xIcup = 20.00m;
            }

            return xIcup;
        }


    }

   


}
