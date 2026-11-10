using System;

namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// The type of cooperative institution a society operates as.
    /// The stored source of truth is <c>Gen_Bank_Name.Bank_Type</c> (int), surfaced at
    /// login into <c>AppState.SocietyType</c> and the "SocietyType" auth claim.
    /// Mapping confirmed by the project owner: 1 = PCARDB, 2 = ECS.
    /// See specs/ecs-extension/01-institution-type.md.
    /// </summary>
    public enum InstitutionType
    {
        /// <summary>Society type could not be determined; treated as the incumbent PCARDB for visibility.</summary>
        Unknown = 0,

        /// <summary>Primary Cooperative Agriculture and Rural Development Bank.</summary>
        PCARDB = 1,

        /// <summary>Cooperative Employees' Thrift and Credit Society.</summary>
        ECS = 2
    }

    public static class InstitutionTypeExtensions
    {
        /// <summary>
        /// Maps a stored <c>Gen_Bank_Name.Bank_Type</c> value to <see cref="InstitutionType"/>.
        /// Unrecognised values map to <see cref="InstitutionType.Unknown"/>.
        /// </summary>
        public static InstitutionType ToInstitutionType(this int bankType) =>
            Enum.IsDefined(typeof(InstitutionType), bankType)
                ? (InstitutionType)bankType
                : InstitutionType.Unknown;
    }
}
