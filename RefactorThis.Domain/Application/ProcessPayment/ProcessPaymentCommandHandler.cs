using MediatR;
using RefactorThis.Domain.Abstractions;
using RefactorThis.Domain.Invoices;
using RefactorThis.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Application.ProcessPayment
{   
    public sealed class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, string>
    {
        private readonly IInvoiceRepository _invoiceRepository;    
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPaymentCommandHandler(
            IInvoiceRepository invoiceRepository,         
            IUnitOfWork unitOfWork)          
        {
            _invoiceRepository = invoiceRepository;           
            _unitOfWork = unitOfWork;           
        }

        public async Task<Result<string>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetInvoice(request.Reference, cancellationToken);

            var result = invoice != null
                ? await ProcessInvoice(invoice, request.Amount)
                : Result.Failure<string>(InvoiceErrors.NotFound);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }

        private async Task<Result<string>> ProcessInvoice(Invoice invoice, decimal paymentAmount)
        {
            switch (true)
            {
                case bool _ when invoice.IsAmountZero:
                    return ProcessAmountZeroCase(invoice);

                case bool _ when invoice.HasPayments:
                    return await ProcessInvoiceWithPayments(invoice, paymentAmount);

                default:
                    return await ProcessInvoiceWithoutPayments(invoice, paymentAmount);
            }
        }

        private Result<string> ProcessAmountZeroCase(Invoice invoice)
        {
            return invoice.HasNoPayments
                ? Result.Failure<string>(InvoiceErrors.NoPaymentNeeded)
                : Result.Failure<string>(InvoiceErrors.InvalidState);
        }

        private async Task<Result<string>> ProcessInvoiceWithPayments(Invoice invoice, decimal paymentAmount)
        {
            return await Task.Run(() =>
            {
                return invoice.IsFullyPaid
                    ? Result.Failure<string>(InvoiceErrors.AlreadyFullyPaid)
                    : invoice.IsPaymentExceedingRemainingAmount(paymentAmount)
                        ? Result.Failure<string>(InvoiceErrors.PaymentExceedsRemainingAmount)
                        : ProcessPartialPayment(invoice, Payment.Create(string.Empty, paymentAmount)).Result;
            });
        }

        private async Task<Result<string>> ProcessInvoiceWithoutPayments(Invoice invoice, decimal paymentAmount)
        {
            return await Task.Run(() =>
            {
                return invoice.IsAmountExceedingRequest(paymentAmount)
                    ? Result.Failure<string>(InvoiceErrors.PaymentExceedsInvoiceAmount)
                    : ProcessFullOrPartialPayment(invoice, Payment.Create(string.Empty, paymentAmount)).Result;
            });
        }

        private async Task<Result<string>> ProcessPartialPayment(Invoice invoice, Payment payment)
        {
            return await Task.Run(() =>
            {
                var message = invoice.IsFinalPayment(payment.Amount)
                    ? ResponseMessages.FinalPartialPaymentReceived
                    : ResponseMessages.AnotherPartialPaymentReceived;

                invoice.AddPayment(payment);
                return Result<string>.Success(message);
            });
        }


        private async Task<Result<string>> ProcessFullOrPartialPayment(Invoice invoice, Payment payment)
        {
            invoice.AddPayment(payment);

            var message = invoice.IsFullPayment(payment.Amount)
                ? ResponseMessages.InvoiceNowFullyPaid
                : ResponseMessages.InvoiceNowPartiallyPaid;

            return await Task.FromResult(Result<string>.Success(message));
        }

    }

}
