using System;
using System.Data.OleDb;
using System.IO;
using System.Numerics;
using System.Text;
using DotNetDBF;
using Migracion;
using Migracion.Clases;
using Npgsql;

class Program
{
    // 12-05-2025 Generado por : CarlosM
    static void Main(string[] args)
    {

        //Variables que contiene las rutas de las tablas de FoxPro
        string dbfFilePath        = @"C:\tmp_archivos\vended.dbf";
        string dbfFileCliente     = @"C:\tmp_archivos\anexosbb.dbf";
        string dbfFileCiudad      = @"C:\tmp_archivos\ciudad.DBF";
        string dbfFileHistoNove   = @"C:\tmp_archivos\histonove.DBF";
        string dbfFileHistonve_P  = @"C:\tmp_archivos\histonove_p.dbf";
        string dbfFileHistonove_S = @"c:\tmp_archivos\histonove_s.dbf";
        string dbfFileitems       = @"c:\tmp_archivos\items.dbf";


        // Son listas que se generar para almacenar todos los datos de las
        // tablas del FoxPro
        var vendedList  = new List<t_vended>();
        var Clien_list  = new List<t_cliente>();
        var ciudade_lis = new List<t_ciudad>();
        var nove_list   = new List<t_histonove>();
        var nove_pen    = new List<t_histonove>();
        var nove_sal    = new List<t_histonove>();

        //Instanciando la clase proceso
        Procesos pro = new Procesos();

        //Provider
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // Solicitando la clase a procesar
        Console.WriteLine("Por favor seleccionar tabla:");
        int xTabla = int.Parse(Console.ReadLine());

        
        switch(xTabla)
        {
            case 1:  // using de vended

                using (FileStream fs = File.OpenRead(dbfFilePath))
                {
                    var reader = new DBFReader(fs);
                    reader.CharEncoding = System.Text.Encoding.UTF8;

                    object[] record;
                    while ((record = reader.NextRecord()) != null)
                    {
                        var vendedor = new t_vended
                        {
                            Vended = record[0]?.ToString().Trim(),
                            Nombre = record[1]?.ToString().Trim(),
                            Cedula = Convert.ToInt32(record[2]),
                            Tel = record[3]?.ToString().Trim(),
                            Direcc = record[4]?.ToString().Trim(),
                            Ciudad = record[5]?.ToString().Trim(),
                            FechIng = Convert.ToDateTime(record[6]),
                            Ccosto = record[7]?.ToString().Trim()
                        };

                        vendedList.Add(vendedor);
                    }
                }

                Procesos.InsertarVendedores(vendedList);

                break;
            case 2:  //using clientes y el de ciudad

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

                Procesos.InsertarClientes(Clien_list, ciudade_lis);

                break;
            case 3: // cambioEps

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

                Procesos.InsertarHistoNove(nove_list);

                break;
            case 4:  // cambioPension

                using (FileStream fc = File.OpenRead(dbfFileHistonve_P))
                {
                    var reader = new DBFReader(fc);
                    reader.CharEncoding = System.Text.Encoding.UTF8;

                    object[] record;
                    while ((record = reader.NextRecord()) != null)
                    {
                        var histonoveP = new t_histonove
                        {
                            cod_ant = record[4]?.ToString()?.Trim(),
                            cod_act = record[5]?.ToString()?.Trim(),
                            usua_reg = record[9]?.ToString()?.Trim(),
                            fech_reg = Convert.ToDateTime(record[7]),
                            hora_reg = TimeOnly.Parse(record[8].ToString()),
                            empleado = record[0]?.ToString()?.Trim(),
                            fech_camb = Convert.ToDateTime(record[6]),
                        };

                        nove_list.Add(histonoveP);

                    }
                }

                Procesos.InsertarHistonoveP(nove_list);


                break;
            case 5:

                using (FileStream fc = File.OpenRead(dbfFileHistonove_S))
                {
                    var reader = new DBFReader(fc);
                    reader.CharEncoding = System.Text.Encoding.UTF8;

                    object[] record;
                    while ((record = reader.NextRecord()) != null)
                    {
                        var histonoveP = new t_histonove
                        {
                            val_ant = Convert.ToInt32(record[2]),
                            val_act = Convert.ToInt32(record[3]),
                            usua_reg = record[9]?.ToString()?.Trim(),
                            fech_reg = Convert.ToDateTime(record[7]),
                            hora_reg = TimeOnly.Parse(record[8].ToString()),
                            empleado = record[0]?.ToString()?.Trim(),
                            fech_camb = Convert.ToDateTime(record[6]),
                        };

                        nove_sal.Add(histonoveP);

                    }
                }

                Procesos.InsertarHistonoveS(nove_sal);

                break;
            case 6:



                break;
        }
        
        
    }

    
}


