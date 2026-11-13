namespace SimpleShop.Api.Exceptions;

// =============================================================================
//  ConflictException  —  a custom exception for BUSINESS RULE violations
// =============================================================================
//  The service throws this when a request is valid JSON but breaks a business
//  rule — for example "a product with this name already exists".
//
//  The ExceptionHandlingMiddleware catches it and turns it into an
//  HTTP "409 Conflict" response. So the service doesn't need to know anything
//  about HTTP — it just says "this is a conflict".
// =============================================================================
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
