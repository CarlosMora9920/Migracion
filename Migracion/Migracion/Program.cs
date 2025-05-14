using System;
using System.Data.OleDb;
using System.IO;
using System.Numerics;
using System.Text;
using DotNetDBF;
using Migracion.Clases;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {

        string dbfFilePath = @"C:\tmp_archivos\vended.dbf";
        string dbfFileCliente = @"C:\tmp_archivos\anexosbb.dbf";
        string dbfFileCiudad = @"C:\tmp_archivos\ciudad.DBF";
        string dbfFileHistoNove = @"C:\tmp_archivos\histonove.DBF";

        var vendedList = new List<t_vended>();
        var Clien_list = new List<t_cliente>();
        var ciudade_lis = new List<t_ciudad>();
        var nove_list = new List<t_histonove>();

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // using de vended
        //using (FileStream fs = File.OpenRead(dbfFilePath))
        //{
        //    var reader = new DBFReader(fs);
        //    reader.CharEncoding = System.Text.Encoding.UTF8;

        //    object[] record;
        //    while ((record = reader.NextRecord()) != null)
        //    {
        //        var vendedor = new t_vended
        //        {
        //            Vended = record[0]?.ToString().Trim(),
        //            Nombre = record[1]?.ToString().Trim(),
        //            Cedula = Convert.ToInt32(record[2]),
        //            Tel = record[3]?.ToString().Trim(),
        //            Direcc = record[4]?.ToString().Trim(),
        //            Ciudad = record[5]?.ToString().Trim(),
        //            FechIng = Convert.ToDateTime(record[6]),
        //            Ccosto = record[7]?.ToString().Trim()
        //        };

        //        vendedList.Add(vendedor);
        //    }
        //}

        //using clientes
        using (FileStream fs = File.OpenRead(dbfFileCliente))
        {
            var reader = new DBFReader(fs);
            reader.CharEncoding = System.Text.Encoding.GetEncoding(1252); // ANSI típico de VFP

            object[] record;
            int fila = 0;
            while (true)
            {
                try
                {
                    record = reader.NextRecord();
                    if (record == null) break;

                    fila++;

                    var cliente = new t_cliente
                    {
                        tdoc = record[0]?.ToString()?.Trim(),
                        anexo = record[1]?.ToString()?.Trim(),
                        nombre = record[2]?.ToString()?.Trim(),
                        dv = record[4]?.ToString()?.Trim(),
                        direcc = record[5]?.ToString()?.Trim(),
                        emailfe1 = record[144]?.ToString()?.Trim(),
                        tel = record[9]?.ToString()?.Trim(),
                        apl1 = record[83]?.ToString()?.Trim(),
                        apl2 = record[84]?.ToString()?.Trim(),
                        nom1 = record[85]?.ToString()?.Trim(),
                        nom2 = record[86]?.ToString()?.Trim(),
                        Dane = record[74]?.ToString()?.Trim(),
                        tipo_per = record[109]?.ToString()?.Trim(),
                        bloqueado = record[12] != null && record[12].ToString() == "1" ? 1 : 0
                    };

                    Clien_list.Add(cliente);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error en fila {fila}: {ex.Message}");
                    continue; // o break si no quieres continuar
                }
            }
        }

        using (FileStream fc = File.OpenRead(dbfFileCiudad))
        {
            var reader = new DBFReader(fc);
            reader.CharEncoding = System.Text.Encoding.UTF8;

            object[] record;
            while ((record = reader.NextRecord()) != null)
            {
                var ciudad = new t_ciudad
                {
                    Municipio = record[1]?.ToString()?.Trim(),
                    Departamento = record[2]?.ToString()?.Trim(),
                    Dane = record[3]?.ToString()?.Trim()
                };

                ciudade_lis.Add(ciudad);

            }
        }


        using (FileStream fc = File.OpenRead(dbfFileHistoNove))
        {
            var reader = new DBFReader(fc);
            reader.CharEncoding = System.Text.Encoding.UTF8;

            object[] record;
            while ((record = reader.NextRecord()) != null)
            {
                var histonove = new t_histonove
                {
                    cod_ant = record[4]?.ToString()?.Trim(),
                    cod_act = record[5]?.ToString()?.Trim(),
                    usua_reg = record[9]?.ToString()?.Trim(),
                    fech_reg = Convert.ToDateTime(record[7]),
                    hora_reg = TimeOnly.Parse(record[8].ToString()),
                    empleado = record[0]?.ToString()?.Trim(),
                    fech_camb = Convert.ToDateTime(record[6]),
                };

                nove_list.Add(histonove);

            }
        }



        //Console.WriteLine($"Total registros leídos: {vendedList.Count}");

        // Aquí llamas tu método de inserción a PostgreSQL


        //InsertarVendedores(vendedList);
       // InsertarClientes(Clien_list, ciudade_lis);
        InsertarHistoNove(nove_list);
    }

    // Método para insertar los registros de "vended" en "vendedores"
    static void InsertarVendedores(List<t_vended> vendedList)
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


    static void InsertarClientes(List<t_cliente> Clien_list, List<t_ciudad> ciudade_lis)
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

    static void InsertarHistoNove(List<t_histonove> nove_list)
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
}


