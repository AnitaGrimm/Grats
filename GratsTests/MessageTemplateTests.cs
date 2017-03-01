using Grats.MessageTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;
using Xunit;

namespace GratsTests
{
    public class MessageTemplateTests
    {
        [Fact]
        public void NullTemplateTextShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(
                () => new MessageTemplate(null));
        }

        [Fact]
        public void NullUserShouldThrow()
        {
            var mt = new MessageTemplate("");
            Assert.Throws<ArgumentNullException>(
                () => mt.Substitute(null));
        }

        [Fact]
        public void LiteralStringShouldNotChange()
        {
            var templateText = "строка без подстановок";
            var mt = new MessageTemplate(templateText);
            var user = new User();
            var resultText = mt.Substitute(user);
            Assert.Equal(templateText, resultText);
        }

        [Fact]
        public void EmptyFieldShouldThrow()
        {
            var templateTexts = new[] {
                "ошибка-> ^ <-ошибка",
                "ошибка-> ^^^ <-ошибка",
            };
            Assert.All(templateTexts, text =>
            {
                Assert.Throws<MessageTemplateSyntaxException>(() => new MessageTemplate(text));
            });
        }

        [Fact]
        public void UnknownFieldShouldThrow()
        {
            var templateText = "^такогополянет";
            Assert.Throws<MessageTemplateSyntaxException>(
                () => new MessageTemplate(templateText));
        }

        [Fact]
        public void DoubleCaretShouldBeEscaped()
        {
            var templateText = @" ^^^^ \ ^^о^^ / ^^^^ ";
            var mt = new MessageTemplate(templateText);
            var user = new User();
            var resultText = mt.Substitute(user);
            Assert.Equal(@" ^^ \ ^о^ / ^^ ", resultText);
        }

        [Fact]
        public void FirstNameShouldSubstitute()
        {
            var templateText = "^имя, ^имя! ^имя";
            var mt = new MessageTemplate(templateText);
            var user = new User
            {
                FirstName = "Иван",
            };
            var resultText = mt.Substitute(user);
            Assert.Equal("Иван, Иван! Иван", resultText);
        }

        [Fact]
        public void FirstNameShouldBeDeclared()
        {
            var templateText = "^имя, ^имя! ^имя";
            var mt = new MessageTemplate(templateText);
            Assert.Equal(ProfileFields.FirstName, mt.Fields);
        }

        [Fact]
        public void NameFieldsShouldWorkTogether()
        {
            var templateText = "Здравствуй, ^имя ^фамилия!";
            var mt = new MessageTemplate(templateText);
            Assert.Equal(
                ProfileFields.FirstName | ProfileFields.LastName,
                mt.Fields);
            var user = new User
            {
                FirstName = "Иван",
                LastName = "Петров",
            };
            var resultText = mt.Substitute(user);
            Assert.Equal("Здравствуй, Иван Петров!", resultText);
        }

        [Fact]
        public void InvalidCaseUseShouldThrow()
        {
            var templateTexts = new[] {
                "^имя{текст}",
                "^пол",
            };
            Assert.All(templateTexts, text =>
            {
                Assert.Throws<MessageTemplateSyntaxException>(() => new MessageTemplate(text));
            });
        }

        [Fact]
        public void InvalidCaseSyntaxShouldThrow()
        {
            var templateTexts = new[] {
                "^пол{}",
                "^пол{ м: абв",
                "^пол{ м: абв, где",
                "^пол{ м: абв:где }",
                "^пол{ м м: абв, где }",
                "^пол{ м1: абв, где }",
                "^пол{ м: абв, где, ж: жзи}",
            };
            Assert.All(templateTexts, text =>
            {
                Assert.Throws<MessageTemplateSyntaxException>(() => new MessageTemplate(text));
            });
        }

        [Fact]
        public void SexShouldSubstitute()
        {
            var templateText = "^пол{м: мужской, ж: женский, не указан}";
            var mt = new MessageTemplate(templateText);
            Assert.Equal(ProfileFields.Sex, mt.Fields);
            var male = new User { Sex = Sex.Male };
            var female = new User { Sex = Sex.Female };
            var unknown = new User();
            Assert.Equal("мужской", mt.Substitute(male));
            Assert.Equal("женский", mt.Substitute(female));
            Assert.Equal("не указан", mt.Substitute(unknown));
        }
    }
}
