namespace ClinicaAcolher.Application.PatientAccess;

/// <summary>O código só aparece nesta resposta, uma única vez.</summary>
public sealed record InvitationIssuedResponse(string Token, DateTimeOffset ExpiresAt);

public sealed record AcceptInvitationRequest(string Token, string Password);