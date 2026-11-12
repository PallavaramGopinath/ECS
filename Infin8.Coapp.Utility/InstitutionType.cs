using System;

namespace Infin8.Coapp.Utility.Enums
{
    /// <summary>
    /// The type of cooperative society a branch operates as.
    /// The stored source of truth is <c>Gen_Bank_Name.Bank_Type</c> (int), surfaced at
    /// login into <c>AppState.SocietyType</c> and the "SocietyType" auth claim.
    /// Mapping: 1 = PCARDB, 2 = Cooperative Housing Society, 3 = ECS (Thrift &amp; Credit).
    /// 0 / unrecognised = Unknown, treated as PCARDB so existing visibility never regresses.
    /// See specs/ecs-extension/07-loan-type-society-type.md (supersedes the mapping in spec 01).
    /// </summary>
    public enum InstitutionType
    {
        /// <summary>Society type could not be determined; treated as the incumbent PCARDB for visibility.</summary>
        Unknown = 0,

        /// <summary>Cooperative Primary Agriculture and Rural Development Bank.</summary>
        PCARDB = 1,

        /// <summary>Cooperative Housing Society.</summary>
        Housing = 2,

        /// <summary>Cooperative (Employees') Thrift and Credit Society.</summary>
        ECS = 3
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
