namespace InterfazHubSpot.Security
{
    public enum SpertaFwkUsuarioAuthFailureKind
    {
        None = 0,
        ConnectionMisconfigured,
        InvalidCredentialsOrUserDisabled,
        CompanyIdRequiredOrInvalid,
        CompanyMismatchFixedUser,
        PerfilRequiredForSharedUser,
        SharedUserTablasCompartidasMismatch,
        CompanyNotInSeguridadPorEmpresa,
        CompanyNotFoundInEmpresas,
        UnexpectedError,
    }
}
