// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.
using System.Windows;
using System.Runtime.InteropServices;
using System;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DbEdit
{
    public class TaskDialog
    {
        [DllImport("comctl32.dll", CharSet = CharSet.Unicode, EntryPoint="TaskDialog")]
        static extern int _TaskDialog(IntPtr hWndParent, IntPtr hInstance, String pszWindowTitle, String pszMainInstruction, String pszContent, int dwCommonButtons, IntPtr pszIcon, out int pnButton);

        #region Modal

        public static TaskDialogResult Show(IWin32Window owner, string text)
        {
            return Show(owner, text, null, null, TaskDialogStandardButtons.Ok);
        }

        public static TaskDialogResult Show(IWin32Window owner, string text, string instruction)
        {
            return Show(owner, text, instruction, null, TaskDialogStandardButtons.Ok, 0);
        }

        public static TaskDialogResult Show(IWin32Window owner, string text, string instruction, string caption)
        {
            return Show(owner, text, instruction, caption, TaskDialogStandardButtons.Ok, 0);
        }

        public static TaskDialogResult Show(IWin32Window owner, string text, string instruction, string caption, TaskDialogStandardButtons buttons)
        {
            return Show(owner, text, instruction, caption, buttons, 0);
        }

        public static TaskDialogResult Show(IWin32Window owner, string text, string instruction, string caption, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            return ShowInternal(owner.Handle, text, instruction, caption, buttons, icon);
        }

        #endregion

        #region Non-modal

        public static TaskDialogResult Show(string text)
        {
            return Show(text, null, null, TaskDialogStandardButtons.Ok);
        }

        public static TaskDialogResult Show(string text, string instruction)
        {
            return Show(text, instruction, null, TaskDialogStandardButtons.Ok, 0);
        }

        public static TaskDialogResult Show(string text, string instruction, string caption)
        {
            return Show(text, instruction, caption, TaskDialogStandardButtons.Ok, 0);
        }

        public static TaskDialogResult Show(string text, string instruction, string caption, TaskDialogStandardButtons buttons)
        {
            return Show(text, instruction, caption, buttons, 0);
        }

        public static TaskDialogResult Show(string text, string instruction, string caption, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            return ShowInternal(IntPtr.Zero, text, instruction, caption, buttons, icon);
        }

        public static MessageBoxResult ShowMsg(Window parent, string text, string instruction = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Warning)
        {
            switch ( ShowMsg( parent, text, instruction, mapMsgBoxButton(buttons), mapMsgBoxImage(icon)))
            {
                case TaskDialogResult.Cancel:
                case TaskDialogResult.Close:
                    return MessageBoxResult.Cancel;
                case TaskDialogResult.No:
                    return  MessageBoxResult.No;
                case TaskDialogResult.Ok:
                    return MessageBoxResult.OK;
                case TaskDialogResult.None:
                    return MessageBoxResult.None;
                case TaskDialogResult.Yes:
                    return MessageBoxResult.Yes;
                default:
                    return MessageBoxResult.None;
            }
        }

        private static TaskDialogStandardIcon mapMsgBoxImage(MessageBoxImage icon)
        {
            switch ( icon )
            {
                case MessageBoxImage.Exclamation:
                    return TaskDialogStandardIcon.Warning;
                case MessageBoxImage.Error:
                    return TaskDialogStandardIcon.Error;
                case MessageBoxImage.Information:
                    return TaskDialogStandardIcon.Information;
                default:
                    return 0;
            }
        }

        private static TaskDialogStandardButtons mapMsgBoxButton(MessageBoxButton buttons)
        {
            switch( buttons )
            {
                case MessageBoxButton.OKCancel:
                    return TaskDialogStandardButtons.Ok | TaskDialogStandardButtons.Cancel;
                case MessageBoxButton.YesNoCancel:
                    return TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No | TaskDialogStandardButtons.Cancel;
                case MessageBoxButton.YesNo:
                    return TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                default:
                    return TaskDialogStandardButtons.Ok;
            }
        }

        public static TaskDialogResult ShowMsg(Window parent, string text, string instruction = "", TaskDialogStandardButtons buttons = TaskDialogStandardButtons.Ok, TaskDialogStandardIcon icon = TaskDialogStandardIcon.Warning)
        {
            return TaskDialog.ShowInternal(parent != null ? new System.Windows.Interop.WindowInteropHelper(parent).Handle : IntPtr.Zero,
                                            instruction,
                                            text,
                                            Resources.Title,
                                            buttons, 
                                            icon);
        }

        public static TaskDialogResult ShowMsg(string text, string instruction = "", TaskDialogStandardButtons buttons = TaskDialogStandardButtons.Ok, TaskDialogStandardIcon icon = TaskDialogStandardIcon.Warning)
        {
            return TaskDialog.ShowInternal(IntPtr.Zero,
                                            instruction,
                                            text,
                                            Resources.Title,
                                            buttons,
                                            icon);
        }

        #endregion

        #region Core implementation

        private static TaskDialogResult ShowInternal(IntPtr owner, string text, string instruction, string caption, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            var td = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog();

            td.OwnerWindowHandle = owner;
            td.Text = text;
            td.InstructionText = instruction;
            td.Caption = caption;
            td.StandardButtons = buttons;
            td.Icon = icon;

            return td.Show();
        }

        #endregion
    }


}
