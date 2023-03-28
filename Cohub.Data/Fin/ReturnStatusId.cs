namespace Cohub.Data.Fin
{
    public enum ReturnStatusId
    {
        /// <summary>
        /// A <see cref="Return"/>'s <see cref="Period.EndDate"/> is in the past.
        /// </summary>
        Payable,
        /// <summary>
        /// A <see cref="Return"/>'s due date is in the past.
        /// </summary>
        Due,
        /// <summary>
        /// A <see cref="Return"/> that will no longer be considered
        /// for applying payment toward.
        /// </summary>
        Closed,
    }
}