using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.Reference );

			var responseMessage = string.Empty;

			if ( inv == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}
			else
			{
				if ( inv.Amount == 0 )
				{
					if ( inv.Payments == null || !inv.Payments.Any( ) )
					{
						responseMessage = "no payment needed";
					}
					else
					{
						throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
					}
				}
				else
				{
					if ( inv.Payments != null && inv.Payments.Any( ) )
					{
						if ( inv.Payments.Sum( x => x.Amount ) != 0 && inv.Amount == inv.Payments.Sum( x => x.Amount ) )
						{
							responseMessage = "invoice was already fully paid";
						}
						else if ( inv.Payments.Sum( x => x.Amount ) != 0 && payment.Amount > ( inv.Amount - inv.AmountPaid ) )
						{
							responseMessage = "the payment is greater than the partial amount remaining";
						}
						else
						{
							if ( ( inv.Amount - inv.AmountPaid ) == payment.Amount )
							{
								switch ( inv.Type )
								{
									case InvoiceType.Standard:
										inv.AmountPaid += payment.Amount;
										inv.Payments.Add( payment );
										responseMessage = "final partial payment received, invoice is now fully paid";
										break;
									case InvoiceType.Commercial:
										inv.AmountPaid += payment.Amount;
										inv.TaxAmount += payment.Amount * 0.14m;
										inv.Payments.Add( payment );
										responseMessage = "final partial payment received, invoice is now fully paid";
										break;
									default:
										throw new ArgumentOutOfRangeException( );
								}
								
							}
							else
							{
								switch ( inv.Type )
								{
									case InvoiceType.Standard:
										inv.AmountPaid += payment.Amount;
										inv.Payments.Add( payment );
										responseMessage = "another partial payment received, still not fully paid";
										break;
									case InvoiceType.Commercial:
										inv.AmountPaid += payment.Amount;
										inv.TaxAmount += payment.Amount * 0.14m;
										inv.Payments.Add( payment );
										responseMessage = "another partial payment received, still not fully paid";
										break;
									default:
										throw new ArgumentOutOfRangeException( );
								}
							}
						}
					}
					else
					{
						if ( payment.Amount > inv.Amount )
						{
							responseMessage = "the payment is greater than the invoice amount";
						}
						else if ( inv.Amount == payment.Amount )
						{
							switch ( inv.Type )
							{
								case InvoiceType.Standard:
									inv.AmountPaid = payment.Amount;
									inv.TaxAmount = payment.Amount * 0.14m;
									inv.Payments.Add( payment );
									responseMessage = "invoice is now fully paid";
									break;
								case InvoiceType.Commercial:
									inv.AmountPaid = payment.Amount;
									inv.TaxAmount = payment.Amount * 0.14m;
									inv.Payments.Add( payment );
									responseMessage = "invoice is now fully paid";
									break;
								default:
									throw new ArgumentOutOfRangeException( );
							}
						}
						else
						{
							switch ( inv.Type )
							{
								case InvoiceType.Standard:
									inv.AmountPaid = payment.Amount;
									inv.TaxAmount = payment.Amount * 0.14m;
									inv.Payments.Add( payment );
									responseMessage = "invoice is now partially paid";
									break;
								case InvoiceType.Commercial:
									inv.AmountPaid = payment.Amount;
									inv.TaxAmount = payment.Amount * 0.14m;
									inv.Payments.Add( payment );
									responseMessage = "invoice is now partially paid";
									break;
								default:
									throw new ArgumentOutOfRangeException( );
							}
						}
					}
				}
			}
			
			inv.Save();

			return responseMessage;
		}
	}
}