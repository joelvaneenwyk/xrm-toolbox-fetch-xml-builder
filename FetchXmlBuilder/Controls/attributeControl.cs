﻿using Rappen.XTB.FetchXmlBuilder.AppCode;
using Rappen.XTB.FetchXmlBuilder.Builder;
using Rappen.XTB.FetchXmlBuilder.ControlsClasses;
using Rappen.XTB.FetchXmlBuilder.DockControls;
using Rappen.XTB.FetchXmlBuilder.Views;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Rappen.XTB.FetchXmlBuilder.Controls
{
    public partial class attributeControl : FetchXmlElementControlBase
    {
        private readonly AttributeMetadata[] attributes;
        private readonly AttributeMetadata[] allattributes;
        private bool aggregate;
        private Cell cell;

        public attributeControl() : this(null, null, null, null)
        {
        }

        public attributeControl(TreeNode node, AttributeMetadata[] attributes, FetchXmlBuilder fetchXmlBuilder, TreeBuilderControl tree)
        {
            InitializeComponent();
            this.attributes = attributes;
            allattributes = fetchXmlBuilder.GetAllAttribues(node.LocalEntityName()).ToArray();
            InitializeFXB(null, fetchXmlBuilder, tree, node);
        }

        protected override void PopulateControls()
        {
            cmbAttribute.Items.Clear();
            aggregate = Node.IsFetchAggregate();
            panAggregate.Visible = aggregate;
            panLayout.Visible = !aggregate;
            cmbAggregate.Enabled = aggregate;
            chkGroupBy.Enabled = aggregate;
            if (!aggregate)
            {
                cmbAggregate.SelectedIndex = -1;
                chkGroupBy.Checked = false;
            }

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    AttributeItem.AddAttributeToComboBox(cmbAttribute, attribute, false, FetchXmlBuilder.friendlyNames);
                }
            }

            cell = fxb.dockControlBuilder.LayoutXML.GetCell(Node);
            if (cell == null)
            {
                cell = fxb.dockControlBuilder.LayoutXML.AddCell(Node);
            }
            if (cell != null)
            {
                chkLayoutVisible.Checked = cell.Width > 0;
                trkLayoutWidth.Value = cell.Width;
            }
            else
            {
                panLayout.Visible = false;
            }
        }

        protected override ControlValidationResult ValidateControl(Control control)
        {
            if (control == cmbAttribute)
            {
                if (string.IsNullOrWhiteSpace(cmbAttribute.Text))
                {
                    return new ControlValidationResult(ControlValidationLevel.Error, "Attribute", ControlValidationMessage.IsRequired);
                }
                if (fxb.entities != null)
                {
                    if (!allattributes.Any(a => a.LogicalName == cmbAttribute.Text))
                    {
                        return new ControlValidationResult(ControlValidationLevel.Warning, "Attribute", ControlValidationMessage.NotInMetadata);
                    }
                    if (!cmbAttribute.Items.OfType<AttributeItem>().Any(a => a.ToString() == cmbAttribute.Text))
                    {
                        return new ControlValidationResult(ControlValidationLevel.Info, "Attribute", ControlValidationMessage.NotShowingNow);
                    }
                }
            }
            else if (control == txtAlias)
            {
                if (Node.IsFetchAggregate() && string.IsNullOrWhiteSpace(txtAlias.Text))
                {
                    return new ControlValidationResult(ControlValidationLevel.Error, "Alias must be specified in aggregate queries");
                }
            }

            return base.ValidateControl(control);
        }

        private void chkGroupBy_CheckedChanged(object sender, EventArgs e)
        {
            EnableAggregateControls();
        }

        private void cmbAggregate_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableAggregateControls();
        }

        private void EnableAggregateControls()
        {
            cmbDateGrouping.Enabled = chkGroupBy.Checked;
            chkDistinct.Enabled = aggregate && !chkGroupBy.Checked;
            if (!chkDistinct.Enabled)
            {
                chkDistinct.Checked = false;
            }
            chkUserTZ.Enabled = chkGroupBy.Checked;
            if (!chkGroupBy.Checked)
            {
                cmbDateGrouping.SelectedIndex = -1;
                chkUserTZ.Checked = false;
            }
        }

        private void helpIcon_Click(object sender, EventArgs e)
        {
            FetchXmlBuilder.HelpClick(sender);
        }

        private void cmbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            fxb.ShowMetadata(Metadata());
            UpdateCell();
        }

        private void UpdateCell()
        {
            trkLayoutWidth.Enabled = chkLayoutVisible.Checked;
            lblWidth.Text = $"Width: {trkLayoutWidth.Value}";
            if (cell == null)
            {
                cell = fxb.dockControlBuilder.LayoutXML.AddCell(Node);
            }
            if (cell != null)
            {
                cell.Name = Node.GetAttributeLayoutName();
                cell.Width = chkLayoutVisible.Checked ? trkLayoutWidth.Value : 0;
                fxb.dockControlLayoutXml?.UpdateXML(cell.Parent?.ToXML());
            }
            panLayout.Visible = cell != null;
        }

        public override MetadataBase Metadata()
        {
            if (cmbAttribute.SelectedItem is AttributeItem item)
            {
                return item.Metadata;
            }
            return base.Metadata();
        }

        public override void Focus()
        {
            cmbAttribute.Focus();
        }

        private void trkLayoutWidth_Scroll(object sender, EventArgs e)
        {
            UpdateCell();
        }

        private void chkLayoutVisible_CheckedChanged(object sender, EventArgs e)
        {
            trkLayoutWidth.Value = chkLayoutVisible.Checked ? 100 : 0;
            UpdateCell();
        }
    }
}