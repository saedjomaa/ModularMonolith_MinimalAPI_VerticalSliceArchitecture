using CourseHub.Shared.Results;

namespace CourseHub.Courses.Domain;

internal static class CourseErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Courses.NotFound", "The course was not found.");

    public static readonly Error NotOwner =
        Error.Forbidden("Courses.NotOwner", "Only the instructor who owns this course can change it."); // CRS-05

    public static readonly Error PriceRequired =
        Error.Conflict("Courses.PriceRequired", "A course cannot be published without a price, and a published course must keep its price."); // CRS-03

    public static readonly Error InstructorRequired =
        Error.Conflict("Courses.InstructorRequired", "A course cannot be published without an instructor."); // CRS-03

    public static readonly Error AlreadyPublished =
        Error.Conflict("Courses.AlreadyPublished", "The course is already published.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict("Courses.ConcurrencyConflict", "The course was changed by another request. Reload it and try again.");

    public static readonly Error InvalidTitle =
        Error.Validation("Courses.InvalidTitle", $"Title is required and must be at most {Course.TitleMaxLength} characters.", "title");

    public static readonly Error InvalidDescription =
        Error.Validation("Courses.InvalidDescription", $"Description must be at most {Course.DescriptionMaxLength} characters.", "description");

    public static readonly Error InvalidPrice =
        Error.Validation("Courses.InvalidPrice", $"Price must be between {Course.MinPrice} and {Course.MaxPrice}.", "price");

    public static readonly Error InvalidCapacity =
        Error.Validation("Courses.InvalidCapacity", $"Capacity must be a whole number between 1 and {Course.MaxCapacity}.", "capacity"); // CRS-04
}
