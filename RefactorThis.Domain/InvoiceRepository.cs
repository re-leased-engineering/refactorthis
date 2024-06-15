namespace RefactorThis.Persistence
{
    public interface IInvoiceRepository
    {
		Invoice GetInvoice(string reference);

		void SaveInvoice(Invoice invoice);

		void Add(Invoice invoice);
    }
}