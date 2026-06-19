using System;
using UnityEngine;
using UnityEngine.Events;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    /// <summary>Fluent action wiring for <see cref="BaUiListRow"/>.</summary>
    public sealed class BaUiListRowBinder
    {
        private readonly BaUiListRow _row;
        private UnityAction _onCenter;
        private UnityAction _onSetDestination;
        private UnityAction _onDrive;
        private UnityAction _onAdd;
        private UnityAction _onDelete;

        internal BaUiListRowBinder(BaUiListRow row) => _row = row;

        public BaUiListRow Row => _row;

        public BaUiListRowBinder Center(UnityAction onClick)
        {
            _onCenter = onClick;
            return this;
        }

        public BaUiListRowBinder SetDestination(UnityAction onClick)
        {
            _onSetDestination = onClick;
            return this;
        }

        public BaUiListRowBinder Drive(UnityAction onClick)
        {
            _onDrive = onClick;
            return this;
        }

        public BaUiListRowBinder Add(UnityAction onClick)
        {
            _onAdd = onClick;
            return this;
        }

        public BaUiListRowBinder Delete(UnityAction onClick)
        {
            _onDelete = onClick;
            return this;
        }

        public BaUiListRowBinder DriveLabel(string text)
        {
            if (_row.DriveLabel != null)
                _row.DriveLabel.text = text;
            return this;
        }

        public BaUiListRowBinder SetDestLabel(string text)
        {
            if (_row.SetDestLabel != null)
                _row.SetDestLabel.text = text;
            return this;
        }

        public BaUiListRowBinder AddLabel(string text)
        {
            if (_row.AddLabel != null)
                _row.AddLabel.text = text;
            return this;
        }

        internal void Apply() =>
            _row.Bind(_onCenter, _onSetDestination, _onDrive, _onAdd, _onDelete);
    }
}
