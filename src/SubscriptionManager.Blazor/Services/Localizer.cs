using System.Globalization;

namespace SubscriptionManager.Blazor.Services;

public sealed class Localizer
{
    public string this[string key] => Get(key);
    public string this[string key, params object[] arguments] => string.Format(CultureInfo.CurrentCulture, Get(key), arguments);

    private static string Get(string key)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var values = language switch
        {
            "en" => English,
            "de" => German,
            _ => Polish
        };

        return values.TryGetValue(key, out var value) ? value : key;
    }

    private static readonly IReadOnlyDictionary<string, string> Polish = new Dictionary<string, string>
    {
        ["Navigation.Overview"] = "Pulpit", ["Navigation.Subscriptions"] = "Subskrypcje",
        ["Layout.Search"] = "Szukaj...", ["Layout.Collapse"] = "Zwiń menu", ["Layout.Expand"] = "Rozwiń menu",
        ["Overview.Eyebrow"] = "PRZEGLĄD", ["Overview.Title"] = "Pulpit", ["Overview.Description"] = "Najważniejsze informacje o cyklicznych wydatkach w jednym miejscu.",
        ["Subscriptions.Eyebrow"] = "KONTROLA CYKLICZNYCH WYDATKÓW", ["Subscriptions.Title"] = "Subskrypcje", ["Subscriptions.Description"] = "Wszystkie subskrypcje, koszty i terminy w jednym miejscu.",
        ["Subscriptions.Add"] = "Dodaj subskrypcję", ["Subscriptions.Active"] = "Aktywne subskrypcje", ["Subscriptions.Monthly"] = "Koszt miesięczny", ["Subscriptions.Yearly"] = "Koszt roczny", ["Subscriptions.MostExpensive"] = "Najdroższa subskrypcja",
        ["Subscriptions.OfAll"] = "z {0} wszystkich", ["Subscriptions.AverageMonthly"] = "Średnio {0} za usługę", ["Subscriptions.AverageYearly"] = "Średnio {0} za usługę rocznie",
        ["Subscriptions.ListTitle"] = "Twoje subskrypcje", ["Subscriptions.ListDescription"] = "Aktywne usługi i najbliższe płatności", ["Subscriptions.Search"] = "Szukaj subskrypcji...",
        ["Filter.Active"] = "Aktywne", ["Filter.All"] = "Wszystkie", ["Filter.Ended"] = "Zakończone", ["Empty.Title"] = "Brak subskrypcji", ["Empty.Description"] = "Dodaj pierwszą subskrypcję, aby rozpocząć.",
        ["Action.Edit"] = "Edytuj", ["Action.End"] = "Zakończ", ["Action.Delete"] = "Usuń", ["Action.Cancel"] = "Anuluj", ["Action.Save"] = "Zapisz", ["Action.Create"] = "Dodaj", ["Action.Close"] = "Zamknij",
        ["Field.Name"] = "Nazwa", ["Field.Amount"] = "Kwota", ["Field.Currency"] = "Waluta", ["Field.BillingPeriod"] = "Okres rozliczeniowy", ["Field.StartDate"] = "Data rozpoczęcia", ["Field.EndDate"] = "Data zakończenia",
        ["Billing.Monthly"] = "Co miesiąc", ["Billing.Quarterly"] = "Co kwartał", ["Billing.SemiAnnual"] = "Co pół roku", ["Billing.Yearly"] = "Co rok",
        ["Dialog.CreateTitle"] = "Dodaj subskrypcję", ["Dialog.CreateDescription"] = "Wprowadź podstawowe dane cyklicznej płatności.", ["Dialog.EditTitle"] = "Edytuj subskrypcję", ["Dialog.EditDescription"] = "Zmień dane subskrypcji.", ["Dialog.EndTitle"] = "Zakończ subskrypcję", ["Dialog.EndDescription"] = "Podaj datę zakończenia subskrypcji.", ["Dialog.DeleteTitle"] = "Usuń subskrypcję", ["Dialog.DeleteDescription"] = "Ta operacja trwale usunie subskrypcję.",
        ["Status.Loading"] = "Ładowanie...", ["Error.Load"] = "Nie udało się pobrać subskrypcji.", ["Error.Save"] = "Nie udało się zapisać zmian.", ["Error.Delete"] = "Nie udało się usunąć subskrypcji.", ["Date.Start"] = "Data rozpoczęcia", ["PerMonth"] = "/ mies.", ["User.Role"] = "Portfolio"
    };

    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        ["Navigation.Overview"] = "Overview", ["Navigation.Subscriptions"] = "Subscriptions", ["Layout.Search"] = "Search...", ["Layout.Collapse"] = "Collapse menu", ["Layout.Expand"] = "Expand menu",
        ["Overview.Eyebrow"] = "OVERVIEW", ["Overview.Title"] = "Overview", ["Overview.Description"] = "Key information about recurring expenses in one place.",
        ["Subscriptions.Eyebrow"] = "RECURRING EXPENSE CONTROL", ["Subscriptions.Title"] = "Subscriptions", ["Subscriptions.Description"] = "Subscriptions, costs and dates in one place.",
        ["Subscriptions.Add"] = "Add subscription", ["Subscriptions.Active"] = "Active subscriptions", ["Subscriptions.Monthly"] = "Monthly cost", ["Subscriptions.Yearly"] = "Yearly cost", ["Subscriptions.MostExpensive"] = "Most expensive subscription",
        ["Subscriptions.OfAll"] = "of {0} total", ["Subscriptions.AverageMonthly"] = "Average {0} per service", ["Subscriptions.AverageYearly"] = "Average {0} per service yearly",
        ["Subscriptions.ListTitle"] = "Your subscriptions", ["Subscriptions.ListDescription"] = "Active services and recurring payments", ["Subscriptions.Search"] = "Search subscriptions...",
        ["Filter.Active"] = "Active", ["Filter.All"] = "All", ["Filter.Ended"] = "Ended", ["Empty.Title"] = "No subscriptions", ["Empty.Description"] = "Add the first subscription to get started.",
        ["Action.Edit"] = "Edit", ["Action.End"] = "End", ["Action.Delete"] = "Delete", ["Action.Cancel"] = "Cancel", ["Action.Save"] = "Save", ["Action.Create"] = "Add", ["Action.Close"] = "Close",
        ["Field.Name"] = "Name", ["Field.Amount"] = "Amount", ["Field.Currency"] = "Currency", ["Field.BillingPeriod"] = "Billing period", ["Field.StartDate"] = "Start date", ["Field.EndDate"] = "End date",
        ["Billing.Monthly"] = "Monthly", ["Billing.Quarterly"] = "Quarterly", ["Billing.SemiAnnual"] = "Semi-annually", ["Billing.Yearly"] = "Yearly",
        ["Dialog.CreateTitle"] = "Add subscription", ["Dialog.CreateDescription"] = "Enter the recurring payment details.", ["Dialog.EditTitle"] = "Edit subscription", ["Dialog.EditDescription"] = "Change subscription details.", ["Dialog.EndTitle"] = "End subscription", ["Dialog.EndDescription"] = "Select the subscription end date.", ["Dialog.DeleteTitle"] = "Delete subscription", ["Dialog.DeleteDescription"] = "This permanently removes the subscription.",
        ["Status.Loading"] = "Loading...", ["Error.Load"] = "Could not load subscriptions.", ["Error.Save"] = "Could not save changes.", ["Error.Delete"] = "Could not delete the subscription.", ["Date.Start"] = "Start date", ["PerMonth"] = "/ mo.", ["User.Role"] = "Portfolio"
    };

    private static readonly IReadOnlyDictionary<string, string> German = new Dictionary<string, string>
    {
        ["Navigation.Overview"] = "Übersicht", ["Navigation.Subscriptions"] = "Abonnements", ["Layout.Search"] = "Suchen...", ["Layout.Collapse"] = "Menü einklappen", ["Layout.Expand"] = "Menü ausklappen",
        ["Overview.Eyebrow"] = "ÜBERSICHT", ["Overview.Title"] = "Übersicht", ["Overview.Description"] = "Die wichtigsten Informationen zu wiederkehrenden Ausgaben an einem Ort.",
        ["Subscriptions.Eyebrow"] = "KONTROLLE WIEDERKEHRENDER AUSGABEN", ["Subscriptions.Title"] = "Abonnements", ["Subscriptions.Description"] = "Abonnements, Kosten und Termine an einem Ort.",
        ["Subscriptions.Add"] = "Abonnement hinzufügen", ["Subscriptions.Active"] = "Aktive Abonnements", ["Subscriptions.Monthly"] = "Monatliche Kosten", ["Subscriptions.Yearly"] = "Jährliche Kosten", ["Subscriptions.MostExpensive"] = "Teuerstes Abonnement",
        ["Subscriptions.OfAll"] = "von insgesamt {0}", ["Subscriptions.AverageMonthly"] = "Durchschnittlich {0} pro Dienst", ["Subscriptions.AverageYearly"] = "Durchschnittlich {0} pro Dienst jährlich",
        ["Subscriptions.ListTitle"] = "Ihre Abonnements", ["Subscriptions.ListDescription"] = "Aktive Dienste und wiederkehrende Zahlungen", ["Subscriptions.Search"] = "Abonnements suchen...",
        ["Filter.Active"] = "Aktiv", ["Filter.All"] = "Alle", ["Filter.Ended"] = "Beendet", ["Empty.Title"] = "Keine Abonnements", ["Empty.Description"] = "Fügen Sie das erste Abonnement hinzu.",
        ["Action.Edit"] = "Bearbeiten", ["Action.End"] = "Beenden", ["Action.Delete"] = "Löschen", ["Action.Cancel"] = "Abbrechen", ["Action.Save"] = "Speichern", ["Action.Create"] = "Hinzufügen", ["Action.Close"] = "Schließen",
        ["Field.Name"] = "Name", ["Field.Amount"] = "Betrag", ["Field.Currency"] = "Währung", ["Field.BillingPeriod"] = "Abrechnungszeitraum", ["Field.StartDate"] = "Startdatum", ["Field.EndDate"] = "Enddatum",
        ["Billing.Monthly"] = "Monatlich", ["Billing.Quarterly"] = "Vierteljährlich", ["Billing.SemiAnnual"] = "Halbjährlich", ["Billing.Yearly"] = "Jährlich",
        ["Dialog.CreateTitle"] = "Abonnement hinzufügen", ["Dialog.CreateDescription"] = "Geben Sie die Daten der wiederkehrenden Zahlung ein.", ["Dialog.EditTitle"] = "Abonnement bearbeiten", ["Dialog.EditDescription"] = "Ändern Sie die Abonnementdaten.", ["Dialog.EndTitle"] = "Abonnement beenden", ["Dialog.EndDescription"] = "Wählen Sie das Enddatum.", ["Dialog.DeleteTitle"] = "Abonnement löschen", ["Dialog.DeleteDescription"] = "Das Abonnement wird dauerhaft gelöscht.",
        ["Status.Loading"] = "Wird geladen...", ["Error.Load"] = "Abonnements konnten nicht geladen werden.", ["Error.Save"] = "Änderungen konnten nicht gespeichert werden.", ["Error.Delete"] = "Abonnement konnte nicht gelöscht werden.", ["Date.Start"] = "Startdatum", ["PerMonth"] = "/ Mon.", ["User.Role"] = "Portfolio"
    };
}
