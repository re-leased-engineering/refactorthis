using MySolution.RefactorThis.Infrastructure.Repositories.Implementations;

namespace MySolution.RefactorThis.Tests.Repositories
{
    [TestClass]
    public class InvoiceRepositoryTests
    {
        [TestMethod]
        public void GetInvoice_Returns_InvoiceInstance()
        {
            var repo = new InvoiceRepository();
            var setInvoice = new Domain.Models.Invoice();
            repo.Add(setInvoice);

            var returnedInvoice = repo.GetInvoice("");
            Assert.IsNotNull(returnedInvoice);
            Assert.IsTrue(returnedInvoice == setInvoice);
        }
    }
}
