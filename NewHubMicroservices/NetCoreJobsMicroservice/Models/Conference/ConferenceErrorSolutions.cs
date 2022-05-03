namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceErrorSolutions
	{
		//ERRO DE VALOR

		public const string ErrorValueArchive = "Considerar Valor do Arquivo";
		public const string ErrorValueArchiveAndCredit = "Considerar Valor do Arquivo e Gerar CRÉDITO para Utilização Futura com a diferença de valores";
		public const string ErrorValueArchiveAndDebit = "Considerar Valor do Arquivo e Gerar DÉBITO para Utilização Futura com a diferença de valores";

		//COBRANCA RETROATIVA

		public const string ErrorRetroactiveUse = "Considerar Valor neste batimento";
		public const string ErrorRetroactiveUseAndCredit = "Considerar valor e gera CRÉDITO para utilização futura";

		//COBRANCA DUPLICADA
		public const string ErrorDuplicateUseAndCredit = "Considerar valor e gera CRÉDITO para utilização futura";

		//BENEFICIARIO BLOQUEADO

		public const string ErrorBeneficiaryBlockUse = "Considerar Valor neste batimento";
		public const string ErrorBeneficiaryBlockUseAndCredit = "Considerar valor e gerar CRÉDITO para utilização futura";

		//REGISTRO SOMENTE NO HUB

		public const string ErrorRegisterHubIgnore = "Desconsiderar Registro";
		public const string ErrorRegisterHubIgnoreAndDebit = "Desconsiderar valor e gera DÉBITO para utilização futura";

		//REGISTRO SOMENTE NO PROVEDOR

		public const string ErrorRegisterProviderUse = "Considerar Valor neste batimento";
		public const string ErrorRegisterProviderUseAndCredit = "Considerar valor e gerar CRÉDITO para utilização futura";

		//BENEFICIARIO EM OUTRA UNIDADE

		public const string ErrorBeneficiaryOtherUnityUse = "Considerar Valor neste batimento";
		public const string ErrorBeneficiaryOtherUnityUseAndCredit = "Considerar valor e gerar CRÉDITO para utilização futura";

		//DIVERGENCIA DE CARTEIRINHA

		public const string ErrorCardUpdate = "Atualizar carteirinha automaticamente";
		public const string ErrorCardIgnore = "Desconsiderar Crítica Cadastral";

		//DIVERGENCIA DE CONTRATO, NOME, CPF, DATA DE NASCIMENTO, PLANO

		public const string ErrorOpenTaskPanel = "Abrir tarefa no painel";
	}
}