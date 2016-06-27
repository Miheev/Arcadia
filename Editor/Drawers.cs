using System;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;
using clojure.lang;

[CustomEditor(typeof(ArcadiaBehaviour), true)]
public class ArcadiaBehaviourEditor : Editor {  
  public static IFn requireFn;
  public static IFn intoArrayFn;
  public static IFn allLoadedUserNamespacesFn;
  public static IFn titleCase;
  public static Symbol editorInteropSymbol;
  
  static ArcadiaBehaviourEditor() {
    requireFn = RT.var("clojure.core", "require");
    requireFn.invoke(Symbol.intern("arcadia.internal.editor-interop"));
    intoArrayFn = RT.var("clojure.core", "into-array");
    allLoadedUserNamespacesFn = RT.var("arcadia.internal.editor-interop", "all-loaded-user-namespaces");
  }
  
  void PopupInspector() {
    EditorGUILayout.Space();
    ArcadiaBehaviour ab = (ArcadiaBehaviour)target;
    if(ab.serializedVar != null) {
      // var, show popup
      var loadedNamespaces = allLoadedUserNamespacesFn.invoke();
      Namespace[] namespaces = (Namespace[])intoArrayFn.invoke(loadedNamespaces);
      var fullyQualifiedVars = namespaces.
      SelectMany(ns => ns.getMappings().
        Select((IMapEntry me) => me.val()).
        Where(v => v.GetType() == typeof(Var) &&
         ((Var)v).Namespace == ns).
        Select(v => v.ToString().Substring(2)));
      
      string[] blank = new string[] { "" };
      string[] popUpItems = blank.Concat(fullyQualifiedVars).ToArray();
      
      int selectedVar = Array.IndexOf(popUpItems, ab.serializedVar);
      if(selectedVar < 0) selectedVar = 0;
      selectedVar = EditorGUILayout.Popup("Function", selectedVar, popUpItems);
      
      ab.serializedVar = popUpItems[selectedVar];
      ab.OnAfterDeserialize();
    } else {
      EditorGUILayout.LabelField("Function", ab.fn == null ? "nil" : ab.fn.ToString());
      
    }
  }
  
  void TextInspector() {
    EditorGUILayout.Space();
    ArcadiaBehaviour ab = (ArcadiaBehaviour)target;
    ab.serializedVar = EditorGUILayout.TextField("Function", ab.serializedVar);
    ab.OnAfterDeserialize();
  }
  
  public override void OnInspectorGUI () {
    var inspectorConfig = ClojureConfiguration.Get("editor", "hooks-inspector");
    
    PopupInspector();
    /*
    if(inspectorConfig == Keyword.intern(null, "popup")) {
      PopupInspector();
    } else if(inspectorConfig == Keyword.intern(null, "text")) {
      TextInspector();
    } else {
      EditorGUILayout.HelpBox("Invalid value for :editor/hooks-inspector in configuration file." +
                              "Expected :text or :drop-down, got " + inspectorConfig.ToString() +
                              ". Showing text inspector.", MessageType.Warning);
      TextInspector();
    }
    */
  }
}

[CustomEditor(typeof(ArcadiaState), true)]
public class ArcadiaStateEditor : Editor {
  static ArcadiaStateEditor() {
    RT.load("arcadia/core");
  }
  
  public override void OnInspectorGUI () {
    ArcadiaBehaviourEditor.requireFn.invoke(Symbol.intern("arcadia.internal.editor-interop"));
    ArcadiaState stateComponent = (ArcadiaState)target;
    RT.var("arcadia.internal.editor-interop", "state-inspector!").invoke(stateComponent.state);
  }
}