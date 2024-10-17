using UniRx;
using NodeTypes;
using System.Collections.Generic;



public class FlowStore 
{
    private static FlowStore _instance;
    public static FlowStore Instance => _instance ?? (_instance = new FlowStore());
 

    public ReactiveCollection<List<Node>> nodes = new ReactiveCollection<List<Node>>();

    // public ReactiveCollection<List<Edge>> edges = new ReactiveCollection<List<Edge>>();

    public ReactiveCollection<List<Node>> nodeList = new ReactiveCollection<List<Node>>();

    public ReactiveCollection<RoutineEdge> edgeList = new ReactiveCollection<RoutineEdge>();

    public ReactiveCollection<string> selectedEdge = new ReactiveCollection<string>();

    public ReactiveCollection<int> selectedEdgeIndex = new ReactiveCollection<int>();

    public ReactiveCollection<string> selectedNode = new ReactiveCollection<string>();

    public  static T GetStore<T>(ReactiveCollection<T> storeVar)
{
    T currentValue = default;
    var unsubscribe = storeVar.ObserveAdd().Subscribe(data =>
    {
        currentValue = data.Value;
    });

    
    
    unsubscribe.Dispose(); // 購読を解除

    return currentValue; // 取得したデータを返す
}
}
