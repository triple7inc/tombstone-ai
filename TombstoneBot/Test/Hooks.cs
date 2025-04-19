using System;
using MelonLoader;
using System.Reflection;
using MelonLoader.NativeUtils;
namespace TombstoneAI.Test
{
    public class Hooks
    {
        public unsafe class CraftHook{
            delegate bool TrySendCraftDelegate(IntPtr instance,IntPtr recipe,int quantity);
            static NativeHook<TrySendCraftDelegate> hook;
            static TrySendCraftDelegate original;
            public static void Setup(Il2CppEntities.Players.Player player){
                try{
                    var method=player.Controller.GetType().GetMethod("TrySendCraft",BindingFlags.Public|BindingFlags.Instance);
                    if(method==null){
                        MelonLogger.Msg("TrySendCraft not found!");
                        return;
                    }
                    IntPtr target=method.MethodHandle.GetFunctionPointer();
                    var replacement=(TrySendCraftDelegate)Hooked;
                    hook=new NativeHook<TrySendCraftDelegate>(target,replacement.GetFunctionPointer());
                    hook.Attach();
                    original=hook.Trampoline;
                    MelonLogger.Msg("TrySendCraft hooked.");
                }catch(Exception e){
                    ///MelonLogger.Msg(e.ToString());
                }
            }
            static bool Hooked(IntPtr instance,IntPtr recipe,int quantity){
                var recipeObj=new Il2CppTileObjects.Crafting.Recipes.Recipe(recipe);
                MelonLogger.Msg($"[Hooked] TrySendCraft: {recipeObj.name} ({recipeObj.Guid})");
                return original(instance,recipe,quantity);
            }
        }
        public unsafe class CmdCraftHook{
            delegate void CmdCraftDelegate(IntPtr instance,Il2CppSystem.String recipeGuid,int quantity);
            static NativeHook<CmdCraftDelegate> hook;
            static CmdCraftDelegate original;
            public static void Setup(Il2CppEntities.Players.Player player){
                try{
                    var method=player.Controller.GetType().GetMethod("CmdCraft",BindingFlags.Public|BindingFlags.Instance);
                    if(method==null){
                        MelonLogger.Msg("CmdCraft not found!");
                        return;
                    }
                    IntPtr target=method.MethodHandle.GetFunctionPointer();
                    var replacement=(CmdCraftDelegate)Hooked;
                    hook=new NativeHook<CmdCraftDelegate>(target,replacement.GetFunctionPointer());
                    hook.Attach();
                    original=hook.Trampoline;
                    MelonLogger.Msg("CmdCraft hooked.");
                }catch(Exception){
                    ///MelonLogger.Msg(e.ToString());
                }
            }
            static void Hooked(IntPtr instance,Il2CppSystem.String recipeGuid,int quantity){
                MelonLogger.Msg($"[Hooked] CmdCraft: {recipeGuid} x{quantity}");
                original(instance,recipeGuid,quantity);
            }
        }
        public unsafe class OpenRecipeHook{
            delegate void OpenRecipeDelegate(IntPtr instance,IntPtr recipe);
            static NativeHook<OpenRecipeDelegate> hook;
            static OpenRecipeDelegate original;
            public static void Setup(){
                try{
                    var type=typeof(Il2CppTombstone.Client.UiJournalRecipeList);
                    var method=type.GetMethod("OpenRecipeInfoPage",BindingFlags.Public|BindingFlags.Instance);
                    if(method==null){
                        MelonLogger.Msg("OpenRecipeInfoPage not found!");
                        return;
                    }
                    IntPtr target=method.MethodHandle.GetFunctionPointer();
                    var replacement=(OpenRecipeDelegate)Hooked;
                    hook=new NativeHook<OpenRecipeDelegate>(target,replacement.GetFunctionPointer());
                    hook.Attach();
                    original=hook.Trampoline;
                    MelonLogger.Msg("OpenRecipeInfoPage hooked.");
                }catch(Exception){
                    ///MelonLogger.Msg(e.ToString());
                }
            }
            static void Hooked(IntPtr instance,IntPtr recipe){
                var recipeObj=new Il2CppTileObjects.Crafting.Recipes.Recipe(recipe);
                MelonLogger.Msg($"[Hooked] OpenRecipeInfoPage: {recipeObj.name} ({recipeObj.Guid})");
                original(instance,recipe);
            }
        }
        public unsafe class JournalRecipeInfoPageSetupHook{
            delegate void OpenRecipeDelegate(IntPtr instance,IntPtr recipe);
            static NativeHook<OpenRecipeDelegate> hook;
            static OpenRecipeDelegate original;
            public static void Setup(Il2CppTombstone.Client.UiJournalRecipeInfoPage page){
                try{
                    var method=page.GetType().GetMethod("SetupIngredients",BindingFlags.Public|BindingFlags.Instance);
                    if(method==null){
                        MelonLogger.Msg("JournalRecipeInfoPage.SetupIngredients not found!");
                        return;
                    }
                    IntPtr target=method.MethodHandle.GetFunctionPointer();
                    var replacement=(OpenRecipeDelegate)Hooked;
                    hook=new NativeHook<OpenRecipeDelegate>(target,replacement.GetFunctionPointer());
                    hook.Attach();
                    original=hook.Trampoline;
                    MelonLogger.Msg("JournalRecipeInfoPage.SetupIngredients hooked.");
                }catch(Exception){
                    ///MelonLogger.Msg(e.ToString());
                }
            }
            static void Hooked(IntPtr instance,IntPtr recipe){
                var recipeObj=new Il2CppTileObjects.Crafting.Recipes.Recipe(recipe);
                MelonLogger.Msg($"[Hooked] JournalRecipeInfoPage.SetupIngredients: {recipeObj.name} ({recipeObj.Guid})");
                original(instance,recipe);
            }
        }
    }
}