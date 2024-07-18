using RefactorThis.Application.Invoices;
using RefactorThis.Application.Payments;
using RefactorThis.Application.Shared.Invoices;
using RefactorThis.Application.Shared.Invoices.DTOs;
using RefactorThis.Application.Shared.Payments;
using RefactorThis.Application.Shared.Payments.DTOs;
using RefactorThis.Domain.Repositories.Interfaces;
using RefactorThis.Infrastructure.Persistence.Repositories;

namespace RefactorThis.Application.Tests;

public class Tests
{
	private IInvoiceRepository _invoiceRepository;
	private IPaymentRepository _paymentRepository;
	
    private IInvoiceService _invoiceService;
    private IPaymentService _paymentService;
    
    [SetUp]
    public void Setup()
    {
	    _invoiceRepository = new InvoiceRepository();
	    _paymentRepository = new PaymentRepository();
	    
        _invoiceService = new InvoiceService(_invoiceRepository);
        _paymentService = new PaymentService(_invoiceRepository, _paymentRepository);
    }

    [Test]
    public async Task ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
    {
        var payment = new ProcessPaymentDto();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _paymentService.ProcessPaymentAsync(payment));
        
        Assert.That( ex.Message, Is.EqualTo("There is no invoice matching this payment"));
    }
    
    [Test]
    public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
    {
	    var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
	    {
		    Amount = 0,
		    AmountPaid = 0
	    });
    
    	var payment = new ProcessPaymentDto
	    {
		    ReferenceKey = invoice
	    };
    
    	var result = await _paymentService.ProcessPaymentAsync(payment);
    
    	Assert.That(result, Is.EqualTo("No payment needed"));
    }
    
    [Test]
    public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
    {
	    var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
	    {
		    Amount = 10,
		    AmountPaid = 10,
		    Payments = new List<PaymentDto>
		    {
			    new() { Amount = 10 }
		    }
	    });
	    
	    var payment = new ProcessPaymentDto
	    {
		    ReferenceKey = invoice
	    };
    
    	var result = await _paymentService.ProcessPaymentAsync(payment);
    
    	Assert.That(result, Is.EqualTo("Invoice was already fully paid"));
    }
    
    [Test]
    public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
    {
	    var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
	    {
		    Amount = 10,
		    AmountPaid = 5,
		    Payments = new List<PaymentDto>
		    {
			    new() { Amount = 5 }
		    }
	    });
	    
	    var payment = new ProcessPaymentDto
	    {
		    ReferenceKey = invoice,
		    Amount = 6
	    };
    
	    var result = await _paymentService.ProcessPaymentAsync(payment);
    
    	Assert.That(result, Is.EqualTo("The payment is greater than the partial amount remaining"));
    }
    
    [Test]
	public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
	{	
		var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
		{
			Amount = 5,
			AmountPaid = 0,
			Payments = new List<PaymentDto>()
		});
	    
		var payment = new ProcessPaymentDto
		{
			ReferenceKey = invoice,
			Amount = 6
		};
    
		var result = await _paymentService.ProcessPaymentAsync(payment);
    
		Assert.That(result, Is.EqualTo("The payment is greater than the invoice amount"));
	}
	
	[Test]
	public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
	{	
		var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
		{
			Amount = 10,
			AmountPaid = 5,
			Payments = new List<PaymentDto>
			{
				new() { Amount = 5 }
			}
		});
	    
		var payment = new ProcessPaymentDto
		{
			ReferenceKey = invoice,
			Amount = 5
		};
    
		var result = await _paymentService.ProcessPaymentAsync(payment);
    
		Assert.That(result, Is.EqualTo("Final partial payment received, invoice is now fully paid"));
	}
	
	[Test]
	public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
	{	
		var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
		{
			Amount = 10,
			AmountPaid = 0,
			Payments = new List<PaymentDto>
			{
				new() { Amount = 10 }
			}
		});
	    
		var payment = new ProcessPaymentDto
		{
			ReferenceKey = invoice,
			Amount = 10
		};
    
		var result = await _paymentService.ProcessPaymentAsync(payment);
    
		Assert.That(result, Is.EqualTo("Invoice was already fully paid"));
	}
	
	[Test]
	public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
	{	
		var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
		{
			Amount = 10,
			AmountPaid = 5,
			Payments = new List<PaymentDto>
			{
				new() { Amount = 5 }
			}
		});
	    
		var payment = new ProcessPaymentDto
		{
			ReferenceKey = invoice,
			Amount = 1
		};
    
		var result = await _paymentService.ProcessPaymentAsync(payment);
    
		Assert.That(result, Is.EqualTo("Another partial payment received, still not fully paid"));
	}
	
	[Test]
	public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
	{	
		var invoice = await _invoiceService.AddAsync(new CreateInvoiceDto
		{
			Amount = 10,
			AmountPaid = 0,
			Payments = new List<PaymentDto>()
		});
	    
		var payment = new ProcessPaymentDto
		{
			ReferenceKey = invoice,
			Amount = 1
		};
    
		var result = await _paymentService.ProcessPaymentAsync(payment);
    
		Assert.That(result, Is.EqualTo("Invoice is now partially paid"));
	}
}