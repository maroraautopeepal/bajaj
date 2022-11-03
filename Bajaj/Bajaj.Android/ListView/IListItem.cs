namespace Bajaj.ListView
{
    public interface IListItem
    {
        ListItemType GetListItemType();

        string Text { get; set; }
    }
}