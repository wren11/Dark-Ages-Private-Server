using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.formulas
{
    [Script("Elements 1.0", "wren", "Default Elemental Table Script used to manage damage mods.")]
    public class Elements : ElementFormulaScript
    {
        public Elements(Sprite obj)
        {

        }

        public override double Calculate(Sprite obj, ElementManager.Element element)
        {
            var defenseElement = obj.DefenseElement;

            while (defenseElement == ElementManager.Element.Random) defenseElement = Sprite.CheckRandomElement(defenseElement);

            if (defenseElement == ElementManager.Element.None && element != ElementManager.Element.None)
                return 1.00;

            if (defenseElement == ElementManager.Element.None && element == ElementManager.Element.None) return 0.50;

            if (defenseElement == ElementManager.Element.Fire)
                switch (element)
                {
                    case ElementManager.Element.Fire:
                        return 0.05;

                    case ElementManager.Element.Water:
                        return 0.85;

                    case ElementManager.Element.Wind:
                        return 0.55;

                    case ElementManager.Element.Earth:
                        return 0.65;

                    case ElementManager.Element.Dark:
                        return 0.75;

                    case ElementManager.Element.Light:
                        return 0.55;

                    case ElementManager.Element.None:
                        return 0.01;
                }

            if (defenseElement == ElementManager.Element.Wind)
                switch (element)
                {
                    case ElementManager.Element.Wind:
                        return 0.05;

                    case ElementManager.Element.Fire:
                        return 0.85;

                    case ElementManager.Element.Water:
                        return 0.65;

                    case ElementManager.Element.Earth:
                        return 0.55;

                    case ElementManager.Element.Dark:
                        return 0.75;

                    case ElementManager.Element.Light:
                        return 0.55;

                    case ElementManager.Element.None:
                        return 0.01;
                }

            if (defenseElement == ElementManager.Element.Earth)
                switch (element)
                {
                    case ElementManager.Element.Wind:
                        return 0.85;

                    case ElementManager.Element.Fire:
                        return 0.65;

                    case ElementManager.Element.Water:
                        return 0.55;

                    case ElementManager.Element.Earth:
                        return 0.05;

                    case ElementManager.Element.Dark:
                        return 0.75;

                    case ElementManager.Element.Light:
                        return 0.55;

                    case ElementManager.Element.None:
                        return 0.01;
                }

            if (defenseElement == ElementManager.Element.Water)
                switch (element)
                {
                    case ElementManager.Element.Wind:
                        return 0.65;

                    case ElementManager.Element.Fire:
                        return 0.55;

                    case ElementManager.Element.Water:
                        return 0.05;

                    case ElementManager.Element.Earth:
                        return 0.85;

                    case ElementManager.Element.Dark:
                        return 0.75;

                    case ElementManager.Element.Light:
                        return 0.55;

                    case ElementManager.Element.None:
                        return 0.01;
                }

            if (defenseElement == ElementManager.Element.Dark)
                return element switch
                {
                    ElementManager.Element.Dark => 0.10,
                    ElementManager.Element.Light => 0.80,
                    ElementManager.Element.None => 0.01,
                    _ => 0.60
                };

            if (defenseElement == ElementManager.Element.Light)
                return element switch
                {
                    ElementManager.Element.Dark => 0.80,
                    ElementManager.Element.Light => 0.10,
                    ElementManager.Element.None => 0.01,
                    _ => 0.65
                };

            return 0.00;
        }
    }
}
