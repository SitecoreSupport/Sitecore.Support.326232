// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecificProductQuantityCondition.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>
//   Defines the condition to compare quantity of specific product with some predefined value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Rules.Conditions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.Commerce.Entities.Carts;
  using Sitecore.Rules;

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
    public uint ProductQuantity { get; set; }

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
  }
}
