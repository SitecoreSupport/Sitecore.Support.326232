﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecificProductQuantityCondition.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>
//   Defines the condition to compare quantity of specific product with some predefined value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Support.Commerce.Rules.Conditions
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using Sitecore.Commerce.Entities.Carts;
  using Sitecore.Commerce.Rules.Conditions;
  using Sitecore.Commerce.Services.Carts;
  using Sitecore.Commerce.Services.Customers;
  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Rules;
  using Sitecore.Rules.Conditions;
  using Sitecore.Sites;

  /// <summary>
  /// Defines the condition to compare the quantity of a product with a predefined value.
  /// The condition follows the following format:
  /// “where the quantity of product [ProductId] in user's cart [Compares to] [ProductQuantity]”
  /// I this format, the [Compares to] clause can be any two-operand comparison operator: <![CDATA[<]]>,<![CDATA[>]]>,<![CDATA[<=]]>,<![CDATA[=]]>,<![CDATA[=>]]>
  /// </summary>
  /// <typeparam name="T">Type of rule context.</typeparam>
  public class SpecificProductQuantityCondition<T> : BaseCartMetricsCondition<T> where T : RuleContext
  {
    /// <summary>
    /// Gets or sets the product id.
    /// </summary>
    /// <value>The product id.</value>
    [NotNull]
    public string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product quantity.
    /// </summary>
    /// <value>The product quantity.</value>
    public decimal ProductQuantity { get; set; }

    #region fixvariableandconstructor
    private CustomerServiceProvider customerServiceProvider;
    

    public SpecificProductQuantityCondition() : base()
    {
      this.customerServiceProvider = (CustomerServiceProvider)Factory.CreateObject("customerServiceProvider", true);
    }
    #endregion

    /// <summary>
    /// Gets the cart metrics.
    /// </summary>
    /// <param name="carts">The carts.</param>
    /// <returns>The metric of the cart.</returns>
    protected override IComparable GetCartMetrics(IEnumerable<Cart> carts)
    {
      return carts.Where(cart => cart != null).Aggregate(0u, (quantity, cart) => quantity + cart.Lines.Where(cartLine => (cartLine != null) && (cartLine.Product != null)).Aggregate(0u, (partialQuantity, cartLine) => partialQuantity + (cartLine.Quantity * Convert.ToByte(cartLine.Product.ProductId == this.ProductId))));
    }

    /// <summary>
    /// Gets the predefined value.
    /// </summary>
    /// <returns>The value to compare with.</returns>
    protected override object GetPredefinedValue()
    {
      return this.ProductQuantity;
    }

    #region fix
    protected override bool Execute(T ruleContext)
    {
      SiteContext shopContext = Context.Site;

      Assert.IsNotNull(shopContext, Sitecore.Commerce.Texts.ContextSiteCannotBeNull);

      string userId = this.GetCurrentUserId();

      GetCartsRequest getCartsRequest = new GetCartsRequest(shopContext.Name) { UserIds = new[] { userId } };
      IEnumerable<Cart> carts = this.CartServiceProvider.GetCarts(getCartsRequest).Carts.Select(cartBase => this.CartServiceProvider.LoadCart(new LoadCartRequest(shopContext.Name, cartBase.ExternalId, userId)).Cart);

      var conditionOperator = this.GetOperator();
      var isConditionMet = false;

      switch (conditionOperator)
      {
        case ConditionOperator.Equal:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) == 0;
          break;
        case ConditionOperator.GreaterThan:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) > 0;
          break;
        case ConditionOperator.GreaterThanOrEqual:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) >= 0;
          break;
        case ConditionOperator.LessThan:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) < 0;
          break;
        case ConditionOperator.LessThanOrEqual:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) <= 0;
          break;
        case ConditionOperator.NotEqual:
          isConditionMet = this.GetCartMetrics(carts).CompareTo(this.GetPredefinedValue()) != 0;
          break;
        default:
          throw new InvalidOperationException(Sitecore.Commerce.Texts.OperatorIsNotSupported);
      }

      Log.Debug(string.Format(CultureInfo.InvariantCulture, "Connect cart condition: userId:{0}, Condition operator: {1}, ConditionMet: {2}, Number of carts found: {3}, Type: {4}", userId, conditionOperator, isConditionMet, (carts != null) ? carts.Count() : 0, this.GetType()));

      return isConditionMet;
    }

    protected virtual string GetCurrentUserId()
    {
      string userName = this.ContactFactory.GetContact();

      if (Sitecore.Context.User.IsAuthenticated)
      {
        var result = this.customerServiceProvider.GetUser(new GetUserRequest(userName));
        if (result != null && result.Success && result.CommerceUser != null)
        {
          userName = result.CommerceUser.ExternalId;
        }
      }

      return userName;
    }
    #endregion
  }
}
