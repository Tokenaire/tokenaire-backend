using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tokenaire.Database.Models;

namespace Tokenaire.Database
{
    public class CustomEntityMaterializerSource : EntityMaterializerSource
    {
        private static readonly MethodInfo NormalizeMethod = typeof(DateTimeMapper)
            .GetTypeInfo()
            .GetMethod(nameof(DateTimeMapper.Normalize));

        public override Expression CreateReadValueExpression(Expression valueBuffer, Type type, int index, IProperty property = null)
        {
            if (type == typeof(DateTime))
            {
                return Expression.Call(
                    NormalizeMethod,
                    base.CreateReadValueExpression(valueBuffer, type, index, property)
                );
            }

            return base.CreateReadValueExpression(valueBuffer, type, index, property);
        }

        public static class DateTimeMapper
        {
            public static DateTime Normalize(DateTime value)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
    }
}

