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
        string dbfFileDocum       = @"j:\businsas\mersas\datos\docum.dbf";
        string dbfFileitems       = @"c:\tmp_archivos\items.dbf";


        // Son listas que se generar para almacenar todos los datos de las
        // tablas del FoxPro
        var vendedList  = new List<t_vended>();
        var Clien_list  = new List<t_cliente>();
        var ciudade_lis = new List<t_ciudad>();
        var nove_list   = new List<t_histonove>();
        var nove_pen    = new List<t_histonove>();
        var nove_sal    = new List<t_histonove>();

        var items_list  = new List<t_items>();

        var docum_list = new List<t_docum>();


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
            case 6://documentos

                using (FileStream fc = File.OpenRead(dbfFileDocum))
                {
                    var reader = new DBFReader(fc);
                    reader.CharEncoding = System.Text.Encoding.UTF8;

                    object[] record;
                    while ((record = reader.NextRecord()) != null)
                    {
                        var documentos = new t_docum
                        {
                            docum = record[]?.ToString()?.Trim(),   //codigo
                            nombre = record[]?.ToString()?.Trim(),  //nombre
                            tipo_doc = record[]?.ToString()?.Trim(),//asimilar
                            contabil = Convert.ToInt32(record[]),   //CD
                            si_cnomb = Convert.ToInt32(record[]),   //CT
                            bloqueado = Convert.ToInt32(record[]),  //IN
                            vali_doc = Convert.ToInt32(record[]),   //VD
                            si_consec = Convert.ToInt32(record[]),  //UV
                            controlrut = Convert.ToInt32(record[]), //CR
                            camb_ter = Convert.ToInt32(record[]),   //PC
                            desc_ord = Convert.ToInt32(record[]),   //DO
                            es_trans = Convert.ToInt32(record[]),   //TR
                            cons_proc = Convert.ToInt32(record[]),  //CA
                            desc_doci = Convert.ToInt32(record[]),  //DD
                            silibtes = Convert.ToInt32(record[]),   //LT
                            n_lineas = Convert.ToInt32(record[]),   //NL, esta campo para nosotros es numerico, ellos lo tienen boleano
                            n_recup = Convert.ToInt32(record[]),    //RD
                            obser_doc = Convert.ToInt32(record[]),  //RO
                            cont_fec = Convert.ToInt32(record[]),   //ControlFechas
                            vend_det = Convert.ToInt32(record[]),   //Vendedor
                            zon_det = Convert.ToInt32(record[]),    //Zona
                            cco_det = Convert.ToInt32(record[]),    //CCosto
                            es_resolu = Convert.ToInt32(record[]),  //Resolucion
                            sniif_on = Convert.ToInt32(record[]),   //ActivarColumna
                            si_contpag = Convert.ToInt32(record[]), //ControlaPagos
                            //Cuentas, no la veo en el business preguntar
                            fecha_cre = Convert.ToDateTime(record[]),//FechaCreacion
                            //IdDocumentoContrapartida
                            //Naturaleza
                            //detalle, se saca de otra tabla llamda detalles
                            Mensaje1 = record[]?.ToString()?.Trim(),//Mensaje1
                            Mensaje2 = record[]?.ToString()?.Trim(),//Mensaje2
                            Mensaje3 = record[]?.ToString()?.Trim(),//Mensaje3
                            afin_cxc = Convert.ToInt32(record[]),   //ValoresCartera
                            Anexo1 = record[]?.ToString()?.Trim(),  //Anexo1
                            Anexo2 = record[]?.ToString()?.Trim(),  //Anexo2
                            Anexo3 = record[]?.ToString()?.Trim(),  //Anexo3
                            Anexo4 = record[]?.ToString()?.Trim(),  //Anexo4
                            Anexo5 = record[]?.ToString()?.Trim(),  //Anexo5
                            Anexo6 = record[]?.ToString()?.Trim(),  //Anexo6
                            afin_tipo = Convert.ToInt32(record[]),  //MovimientoCartera
                            afin_doc = record[]?.ToString()?.Trim(),//FusionarDocumento
                        };
                        docum_list.Add(documentos);
                    }
                }
                break;
            case 7: // items

                using (FileStream fc = File.OpenRead(dbfFileitems))
                {
                    var reader = new DBFReader(fc);
                    reader.CharEncoding = System.Text.Encoding.UTF8;

                    object[] record;
                    while ((record = reader.NextRecord()) != null)
                    {
                        var itemsPick = new t_items
                        {
                            Codigo = record[0]?.ToString()?.Trim(),
                            tipo   = record[12]?.ToString()?.Trim(),
                            fecha_cre = Convert.ToDateTime(record[11]),
                            nombre = record[1]?.ToString()?.Trim(),
                            shortname = record[181]?.ToString()?.Trim(),
                            refabrica = record[86]?.ToString()?.Trim(),
                            peso_uni = Convert.ToInt32(record[61]),
                            undxcaja = Convert.ToInt32(record[57]),
                            subgrupo = record[58]?.ToString()?.Trim(),
                            marca = record[42]?.ToString()?.Trim(),
                            pdrenado = Convert.ToInt32(record[160]),
                            cod_ean8 = record[42]?.ToString()?.Trim(),
                            cod_bar = record[43]?.ToString()?.Trim(),
                            bloqueado = Convert.ToInt32(record[18]),
                            unidad = record[19]?.ToString()?.Trim(),
                            talla = record[87]?.ToString()?.Trim(),
                            linea = record[4]?.ToString()?.Trim(),
                            sublinea = record[5]?.ToString()?.Trim(),
                            no_compra = Convert.ToInt32(record[121]),
                            es_kitpro = Convert.ToInt32(record[138]),
                            iva = Convert.ToInt32(record[6]),
                            pvta1i = Convert.ToInt32(record[7]),
                            pvta_a1 = Convert.ToInt32(record[20]),
                            cambiopv_1= Convert.ToInt32(record[92]),
                            iconsumo = Convert.ToInt32(record[49]),
                            excluido = Convert.ToInt32(record[44]),
                            listap = Convert.ToInt32(record[79]),
                            F_ICONSUMO = Convert.ToInt32(record[135]),
                            costoajus = Convert.ToInt32(record[127]),
                            es_fruver = Convert.ToInt32(record[83]),
                            bolsa = Convert.ToInt32(record[104]),
                            mod_ppos = Convert.ToInt32(record[47]),
                            fenalce = record[103]?.ToString()?.Trim(),
                            acu_tpos = Convert.ToInt32(record[48]),
                            subsidio = Convert.ToInt32(record[105]),
                            mod_qpos = Convert.ToInt32(record[84]),
                            es_bol = Convert.ToInt32(record[132]),
                            contabgrav = Convert.ToInt32(record[96]),
                            sitoledo = Convert.ToInt32(record[88]),
                            pref_ean = record[89]?.ToString()?.Trim(),
                            es_bordado = Convert.ToInt32(record[100]),
                            es_moto = Convert.ToInt32(record[122]),
                            sipedido = Convert.ToInt32(record[136]),
                            escodensa = record[133]?.ToString()?.Trim(),
                            es_ingreso = Convert.ToInt32(record[129]),
                            cheqpr = Convert.ToInt32(record[131]),
                            si_descto = Convert.ToInt32(record[36]),
                            descmax = Convert.ToInt32(record[50]),
                            deci_cant = Convert.ToInt32(record[85]),
                            fech_cp1 = Convert.ToDateTime(record[9]),
                            costo_rep = Convert.ToInt32(record[14]),
                            cod_alt = record[3]?.ToString()?.Trim(),
                            CCosto = record[33]?.ToString()?.Trim(),
                            elegido = Convert.ToInt32(record[55]),
                            sdo_rojo = Convert.ToInt32(record[60]),
                            pidempeso = Convert.ToInt32(record[140]),
                            corrosivo = Convert.ToInt32(record[143]),
                            n_cas = record[141]?.ToString()?.Trim(),
                            cat_cas = record[142]?.ToString()?.Trim(),
                            vta_costo = Convert.ToInt32(record[62]),
                            si_detdoc = Convert.ToInt32(record[69]),
                            solocant = Convert.ToInt32(record[91]),
                            si_serie = Convert.ToInt32(record[125]),
                            terceAutom = Convert.ToInt32(record[126]),
                            generico = Convert.ToInt32(record[161]),
                            pesocajb = Convert.ToInt32(record[63]),
                            pesocajn = Convert.ToInt32(record[64]),
                            peso_car = Convert.ToInt32(record[159]),
                            unidmin = Convert.ToInt32(record[65]),
                            puntaje= Convert.ToInt32(record[112]),
                            fecha1_mp = Convert.ToDateTime(record[113]),
                            fecha2_mp = Convert.ToDateTime(record[114]),
                            desc_esp = Convert.ToInt32(record[107]),
                            fact_esp = Convert.ToInt32(record[108]),
                            valor_esp = Convert.ToInt32(record[109]),
                            fechdesci = Convert.ToDateTime(record[162]),
                            fechdescf = Convert.ToDateTime(record[163]),
                            descod = Convert.ToInt32(record[110]),
                            descod_f = Convert.ToDateTime(record[111]),
                            fchdescusr = Convert.ToDateTime(record[130]),
                            validmax = Convert.ToInt32(record[145]),
                            maxventa = Convert.ToInt32(record[146]),
                            val_desp = Convert.ToInt32(record[148]),
                            f_bloqp = Convert.ToDateTime(record[156]),
                            cont_devol = Convert.ToInt32(record[116]),
                            pvta3i = Convert.ToInt32(record[52]),
                            no_invped = Convert.ToInt32(record[107]),
                            aut_trasl = Convert.ToInt32(record[115]),
                            lp_cyvig = Convert.ToInt32(record[118]),
                            chequeo = Convert.ToInt32(record[106]),
                            costeo2 = Convert.ToInt32(record[119]),
                            sobrestock = Convert.ToInt32(record[123]),
                            Asopanela = Convert.ToInt32(record[124]),
                            grupodes = record[82]?.ToString()?.Trim(),
                            ean_1 = record[80]?.ToString()?.Trim(),
                            ean_2 = record[81]?.ToString()?.Trim(),
                            inandout = Convert.ToInt32(record[155]),
                            domi_com = Convert.ToInt32(record[107]),
                            ord_prio = Convert.ToInt32(record[164]),
                            modq_reg = Convert.ToInt32(record[178]),
                            mod_toler = Convert.ToInt32(record[179]),
                            stockdomi = Convert.ToInt32(record[169]),
                            ext_covid = Convert.ToInt32(record[173]),
                            unidcantfe = record[170]?.ToString()?.Trim(),
                            tipobiend = record[176]?.ToString()?.Trim(),
                            pdiasiva = Convert.ToInt32(record[177]),
                            es_dfnfp = Convert.ToInt32(record[149]),
                            varifnfp = Convert.ToInt32(record[150]),
                            DESFINNIIF = Convert.ToInt32(record[147]),
                            bod_asoc = record[39]?.ToString()?.Trim(),
                            pvtali = Convert.ToInt32(record[7]),
                            descuento = Convert.ToInt32(record[139]),
                            por_rentab = Convert.ToInt32(record[193]),
                            confirpre = Convert.ToInt32(record[137]),
                            ofertado = Convert.ToInt32(record[171]),
                            fech1comp = Convert.ToDateTime(record[144]),
                            vr_imps = Convert.ToInt32(record[196]),
                            cod_ref = record[90]?.ToString()?.Trim(),
                            refer = record[2]?.ToString()?.Trim(),


                        };

                        items_list.Add(itemsPick);

                    }
                }

                break;
                //en el aprendizaje se aprende. cpd
            case 8: //bono devolu catrelina
                 
                break;
        }
        
        
    }

    
}


