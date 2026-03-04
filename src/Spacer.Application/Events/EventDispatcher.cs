namespace Spacer.Application.Events;

using Spacer.Application.Ports;

public sealed class EventDispatcher
{
    private readonly IEventCatalog _catalog;
    private readonly EventRenderer _renderer;
    private readonly EventPreconditionEvaluator _preconditionEvaluator;

    public EventDispatcher(IEventCatalog catalog, EventRenderer renderer, EventPreconditionEvaluator preconditionEvaluator)
    {
        _catalog = catalog;
        _renderer = renderer;
        _preconditionEvaluator = preconditionEvaluator;
    }

    public EventRenderResult? RunById(string id, EventRenderContext context)
    {
        var definition = _catalog.FindById(id);
        if (definition is null)
        {
            return null;
        }

        if (!_preconditionEvaluator.Evaluate(definition.Conditions, context))
        {
            return null;
        }

        return _renderer.Render(definition, context);
    }

    public EventRenderResult? RunChoice(string id, EventRenderContext context, int choiceIndex)
    {
        var definition = _catalog.FindById(id);
        if (definition is null)
        {
            return null;
        }

        return _renderer.RenderChoice(definition, context, choiceIndex);
    }
}
