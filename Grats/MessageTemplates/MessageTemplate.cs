using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet.Model;

namespace Grats.MessageTemplates
{
    public class MessageTemplate : Interfaces.IMessageTemplate
    {
        /// <summary>
        /// Создает новый объект MessageTemplate, основанный на заданном шаблоне
        /// </summary>
        /// <param name="templateText">Текст шаблона</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="MessageTemplateSyntaxException"/>
        public MessageTemplate(string templateText)
        {
            if (templateText == null)
            {
                throw new ArgumentNullException(nameof(templateText));
            }

            int position = 0;
            foreach (Match match in FieldRegex.Matches(templateText))
            {
                // часть шаблона перед или между подстановками
                if (position < match.Index)
                {
                    var text = templateText.Substring(position, match.Index - position);
                    AddLiteralText(text);
                }
                position = match.Index + match.Length;

                // подстановка
                ProcessMatch(match);
            }

            // часть шаблона после последней подстановки
            if (position < templateText.Length)
            {
                var text = templateText.Substring(position);
                AddLiteralText(text);
            }

            Fields = RequiredFields.ToList();
        }

        /// <summary>
        /// Содержит все поля, допустимые для подстановки
        /// </summary>
        public static List<string> AvailableFields { get; }
            = new List<string> {
                "имя",
                "фамилия",
                "пол",
            };

        /// <summary>
        /// Содержит все значения перечисляемых полей
        /// </summary>
        public static Dictionary<string, List<string>> AvailableFieldValues { get; }
            = new Dictionary<string, List<string>> {
                ["пол"] = new List<string>{ "м", "ж", "н" },
            };

        public List<string> Fields { get; }

        public string Substitute(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return string.Concat(TemplateItems.Select(itemFunc => itemFunc(user)));
        }



        private static readonly Regex FieldRegex
            = new Regex(@"
                \^ (
                    ( (?<field> [а-я]+ ) ( { (?<cases> [^}]* ) (?<casesClose> } )? )? )
                    | (?<escape> \^ )?
                )",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private delegate string TemplateItem(User user);

        private HashSet<string> RequiredFields = new HashSet<string>();
        private List<TemplateItem> TemplateItems = new List<TemplateItem>();



        private void ProcessMatch(Match match)
        {
            if (match.Groups["escape"].Success)
            {
                // экранированная ^
                AddLiteralText("^");
            }
            else if (match.Groups["field"].Success)
            {
                // подстановка поля
                var fieldName = match.Groups["field"].Value;

                if (!AvailableFields.Contains(fieldName))
                {
                    throw new MessageTemplateSyntaxException("неизвестное имя поля");
                }

                // часть шаблона для добавления
                TemplateItem templateItem = null;

                // возможные значения для перечисления
                List<string> availableValues;
                if (AvailableFieldValues.TryGetValue(fieldName, out availableValues))
                {
                    if (!match.Groups["cases"].Success)
                    {
                        throw new MessageTemplateSyntaxException("поле требует условную подстановку");
                    }

                    if (!match.Groups["casesClose"].Success)
                    {
                        throw new MessageTemplateSyntaxException("отсутствует закрывающая скобка");
                    }

                    var cases = ParseCases(match.Groups["cases"].Value, availableValues);
                    var defaultText = cases[""];

                    switch (fieldName)
                    {
                        case "пол":
                            RequiredFields.Add("Sex");
                            templateItem = user =>
                            {
                                string value = null;
                                switch (user.Sex)
                                {
                                    case VkNet.Enums.Sex.Male:
                                        value = "м";
                                        break;
                                    case VkNet.Enums.Sex.Female:
                                        value = "ж";
                                        break;
                                    default:
                                        value = "н";
                                        break;
                                }
                                Debug.Assert(value != null);
                                Debug.Assert(AvailableFieldValues[fieldName].Contains(value));
                                string text;
                                return cases.TryGetValue(value, out text) ? text : defaultText;
                            };
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
                else
                {
                    if (match.Groups["cases"].Success)
                    {
                        throw new MessageTemplateSyntaxException("поле не поддерживает условную подстановку");
                    }

                    switch (fieldName)
                    {
                        case "имя":
                            RequiredFields.Add("FirstName");
                            templateItem = user => user.FirstName;
                            break;
                        case "фамилия":
                            RequiredFields.Add("LastName");
                            templateItem = user => user.LastName;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }

                Debug.Assert(templateItem != null);
                TemplateItems.Add(templateItem);
            }
            else
            {
                throw new MessageTemplateSyntaxException("имя поля не указано");
            }
        }

        private void AddLiteralText(string text)
        {
            TemplateItems.Add(user => text);
        }

        private Dictionary<string, string> ParseCases(string casesString, List<string> availableValues)
        {
            var cases = new Dictionary<string, string>();
            string defaultText = null;

            var caseEntries = casesString.Split(',');

            for (int i = 0; i < caseEntries.Length; ++i)
            {
                var entry = caseEntries[i];

                int separatorIndex = entry.IndexOf(':');

                if (separatorIndex < 0)
                {
                    // вместо последней пары может быть текст по умолчанию
                    if (i == caseEntries.Length - 1)
                    {
                        defaultText = entry.Trim();
                    }
                    else
                    {
                        throw new MessageTemplateSyntaxException("текст по умолчанию должен быть последним");
                    }
                }
                else
                {
                    var value = entry.Substring(0, separatorIndex).Trim();
                    if (!Regex.IsMatch(value, "^[а-я]+$"))
                    {
                        throw new MessageTemplateSyntaxException("значение содержит недопустимый символ");
                    }

                    var text = entry.Substring(separatorIndex + 1).Trim();
                    if (!Regex.IsMatch(text, @"^[^,:{}\^]*$"))
                    {
                        throw new MessageTemplateSyntaxException("текст для подстановки содержит недопустимый символ");
                    }

                    if (!availableValues.Contains(value))
                    {
                        throw new MessageTemplateSyntaxException("неизвестное значение поля");
                    }

                    if (cases.ContainsKey(value))
                    {
                        throw new MessageTemplateSyntaxException("повторяющееся значение");
                    }

                    cases[value] = text;
                }
            }

            if (cases.Count == 0)
            {
                throw new MessageTemplateSyntaxException("задан только текст по умолчанию");
            }

            if (cases.Count != availableValues.Count && defaultText == null)
            {
                throw new MessageTemplateSyntaxException("не учтены все возможные значения");
            }

            cases[""] = defaultText;

            return cases;
        }

    }
}
