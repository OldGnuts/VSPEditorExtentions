# VSPEditorExtentions
To use goto Window tab, select VSP Editor Extention

!! Use this at your own risk. Please create a test scene and experiment with the functionality to better understand how to use this.
!! There is limited undo implemented and this will most likely make permanent changes to your scene.
!! This project is still a work in progress however it has been tested and if you find some bugs, let me know

Bulk editing :

    Copy and Paste Settings:
      * The items settings / parameters *
      Paste all settings from the copied item to another item, or all items of a type
      Select settings that you want to paste from the copied item
      Change packages and paste a copied item settings to items in that package
      
    Copy and Paste Items:
      * The item and it's settings into another package *
      Copy items and settings from one package to another
      Either the entire item can be copied or settings can be chosen.
      If settings are left blank and "Copy All Settings" is not selected, it will add the item as if you added it normally.
      
    Delete:
      Delete one or all of type. Type can be all, which will delete all items in a package.
      
    Misc:
      Currently allows enabling or disabling runtime spawn on all items.
      
Import from Terrain:

    Select trees, details or both.
    Note: Detail importing is not a direct import of terrain details. The positions will not be identical however they will be similar.
    
Import from Hierarchy:

    Add items to the "Hierarchy Information" property in the tab. 
    Change the vegetation type to fit the contents of the parent item.
    An example is a parent object named Rocks. It contains rock prefabs. Change its type to Objects or Large Objects.
    
Imports are added to the vegetation package. By default this will check the package first and not duplicate items. 
Imported items instances are stored in persistent vegetation storage.
Instances can be removed by either deleting them from the vegetation package, or they can be removed in the persistent storage component editor.
The items added from hierarch are tagged as "Scene Object Importer" and can be deleted without affecting vegetation that you have baked from rules.
Similarly trees and details are tagged respectively.

It is possible to undo changes made in the Bulk Editor, however not all changes can be undone.
    

      
    
