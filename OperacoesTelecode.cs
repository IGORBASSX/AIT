using System;
using Telecon.Genericos.Classes.BancoDeDados;
using Telecon.GestaoComercial.Biblioteca.Estoque;
using Telecon.GestaoComercial.Biblioteca.Pessoas;
using Pdv = Telecon.GestaoComercial.Biblioteca.PDV.PDV;
using Uni = Telecon.Genericos.Classes.Outros.Universal;

namespace InterfaceTeste
{
    /// <summary>
    /// Operações de inserção feitas pelas classes Telecode da
    /// Telecon.GestaoComercial.Biblioteca, sem executar scripts SQL.
    /// </summary>
    public static class OperacoesTelecode
    {
        private const string ComandoNumeroSerieDisco =
            "EXEC xp_cmdshell 'powershell.exe -NoProfile -Command \"(Get-CimInstance Win32_DiskDrive | Where-Object Index -eq 0).SerialNumber.Trim()\"'";

        #region Cliente / Fornecedor

        public static void InserirCliente(IBanco banco)
        {
            var cliente = new Cliente();
            cliente.Codigo = banco.ObterMaiorCodigo("Clientes", "Codigo") + 1;
            PreencherCliente(cliente);

            VerificarInsercao(banco, Cliente.Inserir(banco, cliente), "o cliente");
        }

        public static void InserirFornecedor(IBanco banco)
        {
            var fornecedor = new Fornecedor();
            fornecedor.Codigo = Convert.ToInt32(banco.ObterMaiorCodigo("Fornecedores", "Codigo")) + 1;
            PreencherFornecedor(fornecedor);

            VerificarInsercao(banco, Fornecedor.Inserir(banco, fornecedor), "o fornecedor");
        }

        #endregion

        #region Produtos

        public static void InserirProdutoArroz(IBanco banco)
        {
            var produto = new Produto();
            produto.Codigo = banco.ObterMaiorCodigo("Produtos", "Codigo") + 1;
            PreencherArroz(produto);

            VerificarInsercao(banco, Produto.Inserir(banco, produto), "o produto Arroz");
        }

        public static void InserirProdutoAbridor(IBanco banco)
        {
            var produto = new Produto();
            produto.Codigo = banco.ObterMaiorCodigo("Produtos", "Codigo") + 1;
            PreencherAbridor(produto);

            VerificarInsercao(banco, Produto.Inserir(banco, produto), "o produto Abridor de latas");
        }

        public static void InserirProdutoCarne(IBanco banco)
        {
            var produto = new Produto();
            produto.Codigo = banco.ObterMaiorCodigo("Produtos", "Codigo") + 1;
            PreencherCarne(produto);

            VerificarInsercao(banco, Produto.Inserir(banco, produto), "o produto Carne");
        }

        public static void InserirProdutoCombustivel(IBanco banco)
        {
            var produto = new Produto();
            produto.Codigo = banco.ObterMaiorCodigo("Produtos", "Codigo") + 1;
            PreencherCombustivel(produto);

            VerificarInsercao(banco, Produto.Inserir(banco, produto), "o produto Combustível");
        }

        #endregion

        #region PDV

        public static void InserirPdv(IBanco banco)
        {
            var pdv = new Pdv();

            pdv.Codigo = Convert.ToInt32(banco.ObterMaiorCodigo("PDVs", "Codigo")) + 1;
            pdv.Modelo = "NFCe";
            pdv.CodLoja = 1;
            pdv.CodPdvKW = 0;
            pdv.NFCe = true;
            pdv.Numero = Convert.ToInt32(banco.ObterMaiorCodigo("PDVs", "Numero")) + 1;
            pdv.NumeroCaixaTerceiros = 0;
            pdv.NumeroSerie = ObterNumeroSerieDisco(banco);
            pdv.PDVTerceiros = false;
            pdv.SelecionarPrecoTabela = false;
            pdv.SerieNFCe = ObterSerieNFCeDisponivel(banco);
            pdv.TorusPdvPos = false;
            pdv.TorusPdvPosNumCaixa = null;

            VerificarInsercao(banco, Pdv.Inserir(banco, pdv), "o PDV");
        }

        /// <summary>
        /// Sorteia uma série NFCe entre 1 e 999 que ainda não exista na tabela PDVs.
        /// </summary>
        private static int ObterSerieNFCeDisponivel(IBanco banco)
        {
            var sorteio = new Random(unchecked(Environment.TickCount * 31 + Guid.NewGuid().GetHashCode()));

            for (var tentativa = 0; tentativa < 999; tentativa++)
            {
                var candidata = sorteio.Next(1, 1000);

                if (Pdv.ConsultarPorSerieNFCe(banco, candidata) == null)
                    return candidata;
            }

            throw new Exception("Todas as séries NFCe de 1 a 999 já estão sendo usadas na tabela PDVs.");
        }

        /// <summary>
        /// Lê o número de série do disco 0 pelo xp_cmdshell, habilitando-o caso necessário.
        /// </summary>
        private static string ObterNumeroSerieDisco(IBanco banco)
        {
            try
            {
                return LerNumeroSerieDisco(banco);
            }
            catch (Exception primeiroErro)
            {
                HabilitarXpCmdShell(banco);

                try
                {
                    return LerNumeroSerieDisco(banco);
                }
                catch (Exception segundoErro)
                {
                    throw new Exception(
                        "Não foi possível obter o número de série do disco 0. " +
                        "Verifique se o xp_cmdshell está habilitado e se o usuário do banco tem permissão para executá-lo. " +
                        "Detalhes: " + segundoErro.Message,
                        primeiroErro);
                }
            }
        }

        private static string LerNumeroSerieDisco(IBanco banco)
        {
            using (var dr = banco.Consultar(ComandoNumeroSerieDisco, 120))
            {
                while (dr.Read())
                {
                    var valor = dr["output"];

                    if (valor == null || valor == DBNull.Value)
                        continue;

                    var texto = Convert.ToString(valor).Trim();

                    if (texto.Length > 0)
                        return texto.Length > 50 ? texto.Substring(0, 50) : texto;
                }
            }

            throw new Exception("O comando de leitura do número de série não retornou nenhum valor.");
        }

        private static void HabilitarXpCmdShell(IBanco banco)
        {
            var comandos = new[]
            {
                "EXEC sp_configure 'show advanced options', 1",
                "RECONFIGURE",
                "EXEC sp_configure 'xp_cmdshell', 1",
                "RECONFIGURE"
            };

            foreach (var comando in comandos)
            {
                try
                {
                    banco.ExecutarComando(comando);
                }
                catch
                {
                    // Sem permissão para alterar a configuração: o erro final explica o problema.
                }
            }
        }

        #endregion

        private static void VerificarInsercao(IBanco banco, bool inserido, string descricao)
        {
            if (inserido)
                return;

            var detalhe = string.IsNullOrEmpty(Uni.UltimoErro) ? string.Empty : " Detalhes: " + Uni.UltimoErro;
            throw new Exception("Não foi possível inserir " + descricao + "." + detalhe);
        }

        #region Valores extraídos dos scripts de teste

        private static void PreencherCliente(Cliente cliente)
        {
            cliente.Nome = "BEBIDAS FRUKI S.A CANOAS";
            cliente.Endereco = "RUA ANTONIO FREDERICO OZANAN, 3030";
            cliente.Bairro = "SAO LUIS";
            cliente.Cidade = "CANOAS";
            cliente.Cpf = "87.315.099/0008-75";
            cliente.Cep = "94420360";
            cliente.Rg = "0240458591";
            cliente.Estado = "RS";
            cliente.Ddd = "51";
            cliente.Fone = "5137481500";
            cliente.Datanasc = "-";
            cliente.Saldo = 0.00m;
            cliente.Gasto = 0.00m;
            cliente.Mae = "-";
            cliente.Pai = "-";
            cliente.Conjugue = "-";
            cliente.Data_mae = "-";
            cliente.Data_pai = "-";
            cliente.Data_conjugue = "-";
            cliente.DiaPagamento = 5;
            cliente.Config = "0110";
            cliente.GRUPO = 1;
            cliente.DT_ULTIMA_VENDA = null;
            cliente.DT_ULTIMO_MOVIMENTO = null;
            cliente.CD_OBS = null;
            cliente.PONTO_REFERENCIA = "-";
            cliente.DT_ADMISSAO = null;
            cliente.RENDA = 0.00m;
            cliente.DT_PRIMEIRA_COMPRA = null;
            cliente.ENDERECO_COBRANCA = "RUA ANTONIO FREDERICO OZANAN, 3030";
            cliente.BAIRRO_COBRANCA = "SAO LUIS";
            cliente.CIDADE_COBRANCA = "CANOAS";
            cliente.UF_COBRANCA = "RS";
            cliente.CEP_COBRANCA = "0";
            cliente.CONTA = "0";
            cliente.AGENCIA = null;
            cliente.CD_BANCO = null;
            cliente.CD_SISTECON = 0;
            cliente.Bloqueado_sistema = false;
            cliente.CodRota = 1;
            cliente.RazaoSocial = "BEBIDAS FRUKI S.A CANOAS";
            cliente.CodVendedor = 1;
            cliente.Ativo = true;
            cliente.CodObsPedidos = null;
            cliente.CodSistemaAntigo = "4617";
            cliente.EMAIL = "LUCIANO.MAXIDEAL@GAMAIL.COM";
            cliente.AvaliarSaldo = true;
            cliente.ReferenciaComercial = null;
            cliente.ConjugeCpf = "-";
            cliente.ConjugeRg = "-";
            cliente.Senha = "-";
            cliente.NumeroFidelidade = "4617";
            cliente.CodMatriz = null;
            cliente.NomeReduzido = "-";
            cliente.FranqueadoSistemaR = false;
            cliente.CodContaSugestao = null;
            cliente.CodContaMovimentacaoSugestao = null;
            cliente.CodFormaPagamentoSugestao = null;
            cliente.CodIdentificacaoLoja = "-";
            cliente.TemBox = false;
            cliente.EmailCobranca = "-";
            cliente.DebitoDiretoAutorizado = false;
            cliente.LimiteCreditoVendasExternasAPP = 0.00m;
            cliente.CodPrazoEcf = 0;
            cliente.CodTitular = null;
            cliente.MaximoParcelas = 1;
            cliente.DiaFechamentofatura = 25;
            cliente.IdIntermediador = null;
            cliente.ContribuinteICMS = "1";
            cliente.Cliente_REF = false;
            cliente.ValorCreditoPrePago = 0.00m;
            cliente.CreditoPrePagoAtivo = false;
        }

        private static void PreencherFornecedor(Fornecedor fornecedor)
        {
            fornecedor.Nome = "BEBIDAS FRUKI S.A CANOAS";
            fornecedor.Fantasia = "FRUKI";
            fornecedor.Endereco = "RUA ANTONIO FREDERICO OZANAN, 3030";
            fornecedor.Cidade = "CANOAS";
            fornecedor.Bairro = "SAO LUIS";
            fornecedor.Cep = "94420360";
            fornecedor.Estado = "RS";
            fornecedor.Cnpj = "87.315.099/0008-75";
            fornecedor.Ie = "0240458591";
            fornecedor.Fone = "5137481500";
            fornecedor.DT_ULTIMA_COMPRA = null;
            fornecedor.DT_ULTIMO_MOVIMENTO = null;
            fornecedor.CHECKBOX_PRODUTOS_DO_FORNECEDOR = false;
            fornecedor.CD_SISTECON = 0;
            fornecedor.TRUNCA_VALOR = false;
            fornecedor.TIPO_FUNRURAL = 0;
            fornecedor.SIMPLES_NACIONAL = false;
            fornecedor.PagaGNRE = false;
            fornecedor.CHECKBOX_CUSTO_GERENCIAL = false;
            fornecedor.CHECKBOX_CFOP_PRODUTO = true;
            fornecedor.ULTIMO_CFOP = 1652;
            fornecedor.CodSistemaAntigo = "4617";
            fornecedor.TIPOPERCREDUCAO = false;
            fornecedor.EMail = "LUCIANO.MAXIDEAL@GAMAIL.COM";
            fornecedor.LancaDescFiscalEmPercentual = false;
            fornecedor.LancaSubstituicaoEmPercentual = false;
            fornecedor.LancaIpiEmPercentual = false;
            fornecedor.OBS = "-";
            fornecedor.AdministradorSistemaR = false;
            fornecedor.Representante = "_";
            fornecedor.CompradorSistemaR = false;
            fornecedor.FornecedorSistemaR = false;
            fornecedor.CodContaSugestao = 59;
            fornecedor.CodContaMovimentacaoSugestao = 4;
            fornecedor.CodFormaPagamentoSugestao = 3;
            fornecedor.PermiteAcessoSistemaRWeb = false;
            fornecedor.CodIdentEstrangeiro = null;
            fornecedor.CodPais = 1058;
            fornecedor.Ativo = true;
            fornecedor.ConferirNfEntradas = true;
            fornecedor.ManifestarAutomatico = 0;
            fornecedor.UsaRetencao = false;
            fornecedor.CodServicoPadrao = null;
            fornecedor.ValorBrutoPadrao = 0.00m;
            fornecedor.ContribuinteICMS = "1";
        }

        private static void PreencherArroz(Produto produto)
        {
            produto.Descricao = "TIO JOAO ARROZ PARBOILIZADO 1KG";
            produto.DescricaoReduzida = "TIO JOAO ARROZ PARBOILIZADO 1";
            produto.Grupo = "02.03.02";
            produto.Unidade = 1;
            produto.ValorVenda = 9.99m;
            produto.MargemLucro = 25.00m;
            produto.Custo = 2.17m;
            produto.Qtd = 445.00m;
            produto.QtdMinima = 50.00m;
            produto.Aliquota = 7;
            produto.Config = "10001100";
            produto.Validade = 0;
            produto.IPI = 0.00m;
            produto.QT_EMBALAGEM = 1.00m;
            produto.CD_ASSOCIADO = 2189;
            produto.ICMS_COMPRA = 12.00m;
            produto.CD_APLICACAO = null;
            produto.VALOR_NO_PDV = 9.99m;
            produto.ST = "020";
            produto.BASE_REDUZIDA_ICMS = 58.333m;
            produto.Custo_Gerencial = 2.33m;
            produto.CnpjCliente = "-";
            produto.CFOP = 5102;
            produto.LOCALIZACAO = "-";
            produto.CD_SITUACAO_PRODUTO = 1;
            produto.PesoBruto = 1.015m;
            produto.CodAliquotaPisCofins = 4;
            produto.NCM = "10063021";
            produto.MVA = 0.00m;
            produto.Markup = 40.00m;
            produto.VINCULARPRECOVENDACOMITENSFORMULA = false;
            produto.FatorCustoDePaiParaFilho = false;
            produto.VincularCustoGerencialTotalItens = false;
            produto.CodSistemaAntigo = "19392";
            produto.Qtd_Embalagem_Venda = "-";
            produto.Csosn = "500";
            produto.CODVASILHAME = null;
            produto.CodSecao = "01";
            produto.CodFornecedorTabela = null;
            produto.CodUltimaClassificacao = 15114998;
            produto.ExcluirNaManutencao = false;
            produto.NaturezaReceita = 105;
            produto.AliquotaIcmsNFSaidas = 12.00m;
            produto.CodigoUltimoRecalcularEstoque = 0;
            produto.IgnoraBCClient = false;
            produto.BloqueiaMultiplicacao = false;
            produto.EX_Tipi = "000";
            produto.ProdutoCaixa = false;
            produto.UsaRefUndBasica = true;
            produto.QtdRefUndBasica = 1.00m;
            produto.CodRefUndBasica = 1;
            produto.CodigoCest = "0000000";
            produto.PercentualDiferimento = 0.00m;
            produto.CodTipoFormula = 1;
            produto.CodTipoItem0200SPED = "00";
            produto.EscalaNaoRelevante = null;
            produto.CNPJFabricante = null;
            produto.FundoCombateaPobreza = false;
            produto.NaoUsaDepartamentoBalanca = false;
            produto.AliquotaICMSST = 0.00m;
            produto.AliquotaFCPST = 0.00m;
            produto.BaseReduzidaST = 0.00m;
            produto.PropagaBaseSTCheiaFormulas = false;
            produto.PermitirVenderProdEstNegativoTerceiro = false;
            produto.ControlaValidade = false;
            produto.ValidadeMinRecebimento = 0;
            produto.DiasValidade = 0;
            produto.AproveitaStBlocoE = false;
            produto.DifereCemPorcNaoCons = false;
            produto.PreEmbalado = false;
            produto.PesagemEstoqueSeguranca = 0;
            produto.IgnoraConferenciaDePeso = false;
            produto.Tara = 2m;
        }

        private static void PreencherAbridor(Produto produto)
        {
            produto.Descricao = "TRAMONTINA ABRIDOR DE LATAS INOX MARFIM";
            produto.DescricaoReduzida = "TRAMONTINA ABRIDOR DE LATAS I";
            produto.Grupo = "17.06";
            produto.Unidade = 1;
            produto.ValorVenda = 55.99m;
            produto.MargemLucro = 0.00m;
            produto.Custo = null;
            produto.Qtd = 0.00m;
            produto.QtdMinima = 0.00m;
            produto.Aliquota = 1;
            produto.Config = "10000000";
            produto.Validade = 0;
            produto.IPI = 0.00m;
            produto.DT_ULTIMA_VENDA = null;
            produto.QT_EMBALAGEM = 1.00m;
            produto.CD_ASSOCIADO = null;
            produto.ICMS_COMPRA = 0.00m;
            produto.CD_APLICACAO = null;
            produto.VALOR_NO_PDV = 55.99m;
            produto.ST = "040";
            produto.BASE_REDUZIDA_ICMS = 0.00m;
            produto.Custo_Gerencial = 0.00m;
            produto.CnpjCliente = "09.564.091/0001-62";
            produto.CFOP = 5102;
            produto.LOCALIZACAO = "-";
            produto.CD_SITUACAO_PRODUTO = 1;
            produto.DT_ULT_ALTERACAO = null;
            produto.PesoBruto = 0.00m;
            produto.CodAliquotaPisCofins = 1;
            produto.NCM = "82055100";
            produto.MVA = 0.00m;
            produto.Markup = 0.00m;
            produto.VINCULARPRECOVENDACOMITENSFORMULA = false;
            produto.FatorCustoDePaiParaFilho = false;
            produto.VincularCustoGerencialTotalItens = false;
            produto.CodSistemaAntigo = "-";
            produto.Qtd_Embalagem_Venda = "-";
            produto.Csosn = "102";
            produto.CODVASILHAME = null;
            produto.CodSecao = "01";
            produto.CodFornecedorTabela = null;
            produto.CodUltimaClassificacao = 1;
            produto.ExcluirNaManutencao = false;
            produto.NaturezaReceita = 0;
            produto.AliquotaIcmsNFSaidas = 0.00m;
            produto.CodigoUltimoRecalcularEstoque = 0;
            produto.IgnoraBCClient = false;
            produto.BloqueiaMultiplicacao = false;
            produto.EX_Tipi = "000";
            produto.ProdutoCaixa = false;
            produto.UsaRefUndBasica = false;
            produto.QtdRefUndBasica = 1.00m;
            produto.CodRefUndBasica = 12;
            produto.CodigoCest = "0801000";
            produto.PercentualDiferimento = 0.00m;
            produto.CodTipoFormula = 1;
            produto.CodTipoItem0200SPED = "00";
            produto.EscalaNaoRelevante = null;
            produto.CNPJFabricante = null;
            produto.FundoCombateaPobreza = false;
            produto.NaoUsaDepartamentoBalanca = false;
            produto.AliquotaICMSST = 0.00m;
            produto.AliquotaFCPST = 0.00m;
            produto.BaseReduzidaST = 0.00m;
            produto.PropagaBaseSTCheiaFormulas = false;
            produto.PermitirVenderProdEstNegativoTerceiro = false;
            produto.ControlaValidade = false;
            produto.ValidadeMinRecebimento = 0;
            produto.DiasValidade = 0;
            produto.AproveitaStBlocoE = false;
            produto.DifereCemPorcNaoCons = false;
            produto.PreEmbalado = false;
            produto.PesagemEstoqueSeguranca = 0;
            produto.IgnoraConferenciaDePeso = false;
            produto.Tara = 2m;
        }

        private static void PreencherCarne(Produto produto)
        {
            produto.Descricao = "CBR COSTELA JANELA KG";
            produto.DescricaoReduzida = "CBR COSTELA JANELA KG";
            produto.Grupo = "12.02";
            produto.Unidade = 2;
            produto.ValorVenda = 38.90m;
            produto.MargemLucro = 0.00m;
            produto.Custo = 10.78m;
            produto.Qtd = 66174.5301m;
            produto.QtdMinima = 60.00m;
            produto.Aliquota = 3;
            produto.Config = "11001100";
            produto.Validade = 6;
            produto.IPI = 0.00m;
            produto.QT_EMBALAGEM = 1.00m;
            produto.CD_ASSOCIADO = 493;
            produto.ICMS_COMPRA = 0.00m;
            produto.CD_APLICACAO = null;
            produto.VALOR_NO_PDV = 33.90m;
            produto.ST = "060";
            produto.BASE_REDUZIDA_ICMS = 0.00m;
            produto.Custo_Gerencial = 10.78m;
            produto.CnpjCliente = "";
            produto.CFOP = 5405;
            produto.LOCALIZACAO = "-";
            produto.CD_SITUACAO_PRODUTO = 1;
            produto.PesoBruto = 0.00m;
            produto.CodAliquotaPisCofins = 4;
            produto.NCM = "02062100";
            produto.MVA = 30.00m;
            produto.Markup = 35.00m;
            produto.VINCULARPRECOVENDACOMITENSFORMULA = false;
            produto.FatorCustoDePaiParaFilho = false;
            produto.VincularCustoGerencialTotalItens = false;
            produto.CodSistemaAntigo = "51";
            produto.Qtd_Embalagem_Venda = "-";
            produto.Csosn = "102";
            produto.CODVASILHAME = null;
            produto.CodSecao = "01";
            produto.CodFornecedorTabela = null;
            produto.CodUltimaClassificacao = 0;
            produto.ExcluirNaManutencao = false;
            produto.NaturezaReceita = 121;
            produto.AliquotaIcmsNFSaidas = 0.00m;
            produto.CodigoUltimoRecalcularEstoque = 0;
            produto.IgnoraBCClient = false;
            produto.BloqueiaMultiplicacao = false;
            produto.EX_Tipi = "000";
            produto.ProdutoCaixa = false;
            produto.UsaRefUndBasica = true;
            produto.QtdRefUndBasica = 1.00m;
            produto.CodRefUndBasica = 1;
            produto.CodigoCest = "1708400";
            produto.PercentualDiferimento = 0.00m;
            produto.CodTipoFormula = 1;
            produto.CodTipoItem0200SPED = "00";
            produto.EscalaNaoRelevante = null;
            produto.CNPJFabricante = null;
            produto.FundoCombateaPobreza = false;
            produto.NaoUsaDepartamentoBalanca = false;
            produto.AliquotaICMSST = 12.00m;
            produto.AliquotaFCPST = 0.00m;
            produto.BaseReduzidaST = 58.333m;
            produto.PropagaBaseSTCheiaFormulas = true;
            produto.PermitirVenderProdEstNegativoTerceiro = false;
            produto.ControlaValidade = false;
            produto.ValidadeMinRecebimento = 0;
            produto.DiasValidade = 0;
            produto.AproveitaStBlocoE = false;
            produto.DifereCemPorcNaoCons = false;
            produto.PreEmbalado = false;
            produto.PesagemEstoqueSeguranca = 0;
            produto.IgnoraConferenciaDePeso = false;
            produto.Tara = 2m;
        }

        private static void PreencherCombustivel(Produto produto)
        {
            produto.Descricao = "GASOLINA COMUM";
            produto.DescricaoReduzida = "GASOLINA COMUM";
            produto.Grupo = "17.03";
            produto.Unidade = 1;
            produto.ValorVenda = 3.93m;
            produto.MargemLucro = 80.00m;
            produto.Custo = 4.04m;
            produto.Qtd = 0.00m;
            produto.QtdMinima = 0.00m;
            produto.Aliquota = 3;
            produto.Config = "10000000";
            produto.Validade = 0;
            produto.IPI = 0.00m;
            produto.DT_ULTIMA_VENDA = null;
            produto.QT_EMBALAGEM = 1.00m;
            produto.CD_ASSOCIADO = null;
            produto.ICMS_COMPRA = 0.00m;
            produto.CD_APLICACAO = null;
            produto.VALOR_NO_PDV = 3.93m;
            produto.ST = "060";
            produto.BASE_REDUZIDA_ICMS = 0.00m;
            produto.Custo_Gerencial = 4.04m;
            produto.CnpjCliente = "";
            produto.CFOP = 5405;
            produto.LOCALIZACAO = "-";
            produto.CD_SITUACAO_PRODUTO = 1;
            produto.PesoBruto = 0.00m;
            produto.CodAliquotaPisCofins = 1;
            produto.NCM = "27101259";
            produto.MVA = 0.00m;
            produto.Markup = 80.00m;
            produto.VINCULARPRECOVENDACOMITENSFORMULA = false;
            produto.FatorCustoDePaiParaFilho = false;
            produto.VincularCustoGerencialTotalItens = false;
            produto.CodSistemaAntigo = "57682";
            produto.Qtd_Embalagem_Venda = "-";
            produto.Csosn = "102";
            produto.CODVASILHAME = null;
            produto.CodSecao = "01";
            produto.CodFornecedorTabela = null;
            produto.CodUltimaClassificacao = 0;
            produto.ExcluirNaManutencao = false;
            produto.NaturezaReceita = 0;
            produto.AliquotaIcmsNFSaidas = 0.00m;
            produto.CodigoUltimoRecalcularEstoque = 0;
            produto.IgnoraBCClient = false;
            produto.BloqueiaMultiplicacao = false;
            produto.EX_Tipi = "000";
            produto.ProdutoCaixa = false;
            produto.UsaRefUndBasica = false;
            produto.QtdRefUndBasica = 1.00m;
            produto.CodRefUndBasica = 1;
            produto.CodigoCest = "0600200";
            produto.PercentualDiferimento = 0.00m;
            produto.CodTipoFormula = 1;
            produto.CodTipoItem0200SPED = "07";
            produto.EscalaNaoRelevante = null;
            produto.CNPJFabricante = null;
            produto.FundoCombateaPobreza = false;
            produto.NaoUsaDepartamentoBalanca = false;
            produto.AliquotaICMSST = 17.00m;
            produto.AliquotaFCPST = 0.00m;
            produto.BaseReduzidaST = 0.00m;
            produto.PropagaBaseSTCheiaFormulas = false;
            produto.PermitirVenderProdEstNegativoTerceiro = false;
            produto.ControlaValidade = false;
            produto.ValidadeMinRecebimento = 0;
            produto.DiasValidade = 0;
            produto.AproveitaStBlocoE = false;
            produto.DifereCemPorcNaoCons = false;
            produto.PreEmbalado = false;
            produto.PesagemEstoqueSeguranca = 0;
            produto.IgnoraConferenciaDePeso = false;
            produto.Tara = 2m;
        }

        #endregion
    }
}
