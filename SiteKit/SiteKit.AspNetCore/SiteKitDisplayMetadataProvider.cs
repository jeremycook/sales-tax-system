using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace SiteKit.AspNetCore
{
    public class SiteKitDisplayMetadataProvider : IDisplayMetadataProvider
    {
        private static readonly Type[] collectionInterfaceTypes = new[] { typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>) };

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.ContainerType == null || context.Key.PropertyInfo == null)
            {
                return;
            }

            var attributes = context.Attributes;
            var metadata = context.DisplayMetadata;
            var property = context.Key.PropertyInfo;
            var propertyName = context.Key.Name;

            if (propertyName != null && IsMissingDisplayName(propertyName, metadata, attributes))
            {
                if (IsCollection(context, out var _))
                {
                    metadata.DisplayName = () =>
                    {
                        return propertyName.Humanize(LetterCasing.Title).Pluralize(inputIsKnownToBeSingular: false);
                    };
                }
                else if (propertyName == "Id")
                {
                    metadata.DisplayName = () =>
                    {
                        return "ID";
                    };
                }
                else
                {
                    metadata.DisplayName = () =>
                    {
                        return Regex.Replace(propertyName, @"Id$", "").Humanize(LetterCasing.Title);
                    };
                }
            }

            if (metadata.DataTypeName == null &&
                metadata.TemplateHint == null)
            {
                //if (property.PropertyType.IsEnum)
                //{
                //    metadata.DataTypeName = "Ënum";
                //    metadata.TemplateHint = property.PropertyType.Name;
                //}
                //else 
                if (IsCollection(context, out var collectionType) && collectionType != null)
                {
                    metadata.TemplateHint = $"{context.Key.ContainerType.Name}.{property.Name}";
                    metadata.DataTypeName = collectionType.GetGenericArguments()[0].Name + "List";
                }

                if (attributes.OfType<ColumnAttribute>().FirstOrDefault() is ColumnAttribute columnAttribute)
                {
                    switch (columnAttribute.TypeName)
                    {
                        case null:
                            break;
                        case "date":
                            metadata.DataTypeName = "Date";
                            break;
                        default:
                            if (columnAttribute.TypeName.StartsWith("decimal"))
                            {
                                metadata.DataTypeName = "Decimal";
                            }
                            break;
                    }
                }
            }

            if (attributes.OfType<EditableAttribute>().FirstOrDefault() is EditableAttribute editableAttribute)
            {
                metadata.ShowForEdit = editableAttribute.AllowEdit;
            }
            else if (metadata.ShowForEdit && property.GetSetMethod() == null)
            {
                metadata.ShowForEdit = false;
            }
        }

        private static bool IsCollection(DisplayMetadataProviderContext context, out Type? collectionType)
        {
            if (context.Key.PropertyInfo != null)
            {
                if (context.Key.PropertyInfo.PropertyType.GetInterfaces().FirstOrDefault(i =>
                     i.IsGenericType &&
                    collectionInterfaceTypes.Contains(i.GetGenericTypeDefinition())
                ) is Type type)
                {
                    collectionType = type;
                    return true;
                }
            }

            collectionType = null;
            return false;
        }

        private static bool IsMissingDisplayName(string propertyName, DisplayMetadata modelMetadata, IReadOnlyList<object> attributes)
        {
            if (!string.IsNullOrEmpty(modelMetadata.SimpleDisplayProperty))
                return false;

            if (attributes.OfType<DisplayNameAttribute>().Any(o => o.DisplayName != null))
                return false;

            if (attributes.OfType<DisplayAttribute>().Any(o => o.Name != null))
                return false;

            if (string.IsNullOrEmpty(propertyName))
                return false;

            return true;
        }
    }
}
