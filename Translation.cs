using Rocket.API.Collections;


namespace ExampleUnturnedPlugin
{
    public static class Translation
    {
        public static TranslationList DefaultTranslations = new TranslationList()
        {
            {"banplayer", "**Игрок [{0}]({1}) был забанен!\n Время бана: {2}\n Кто забанил: [{3}]({4})\nСервер: {5}**" },
            {"banplayer_connected", "**Забаненный игрок [{0}]({1}) пытался зайти \nСервер: {2}**" },
            {"unbanplayer", "**[{0}]({1}) был разбанен!\n Кто разбанил: [{2}]({3})\nСервер: {4}**" }
        };
    }
}