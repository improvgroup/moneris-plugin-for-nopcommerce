using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Moneris.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Moneris.Controllers
{
    public class PaymentMonerisController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly MonerisPaymentSettings _monerisPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        #endregion

        #region Ctor

        public PaymentMonerisController(ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            MonerisPaymentSettings monerisPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._monerisPaymentSettings = monerisPaymentSettings;
            this._paymentSettings = paymentSettings;
        }

        #endregion

        #region Methods

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
                            {
                                AdditionalFee = _monerisPaymentSettings.AdditionalFee,
                                AdditionalFeePercentage = _monerisPaymentSettings.AdditionalFeePercentage,
                                HppKey = _monerisPaymentSettings.HppKey,
                                PsStoreId = _monerisPaymentSettings.PsStoreId,
                                UseSandbox = _monerisPaymentSettings.UseSandbox
                            };

            return View("~/Plugins/Payments.Moneris/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _monerisPaymentSettings.AdditionalFee = model.AdditionalFee;
            _monerisPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            _monerisPaymentSettings.HppKey = model.HppKey;
            _monerisPaymentSettings.PsStoreId = model.PsStoreId;
            _monerisPaymentSettings.UseSandbox = model.UseSandbox;
            _settingService.SaveSetting(_monerisPaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.Moneris/Views/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult SuccessCallbackHandler()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Moneris") as MonerisPaymentProcessor;
            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
            {
                throw new NopException("Moneris module cannot be loaded");
            }

            if (Request.Params != null && Request.Params.Count > 0)
            {
                if (Request.Params.AllKeys.Contains("transactionKey") &&
                    Request.Params.AllKeys.Contains("rvar_order_id"))
                {
                    var transactionKey = Request.Params["transactionKey"];
                    Dictionary<string, string> values;
                    if (processor.TransactionVerification(transactionKey, out values))
                    {
                        var orderIdValue = Request.Params["rvar_order_id"];
                        int orderId;
                        if (int.TryParse(orderIdValue, out orderId))
                        {
                            var order = _orderService.GetOrderById(orderId);
                            if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                            {
                                if (values.ContainsKey("txn_num"))
                                {
                                    order.AuthorizationTransactionId = values["txn_num"];
                                    _orderService.UpdateOrder(order);
                                }

                                _orderProcessingService.MarkOrderAsPaid(order);
                                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [ValidateInput(false)]
        public ActionResult FailCallbackHandler()
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion
    }
}