using Punfai.Report.Fillers;
using Punfai.Report.Template;
using System.Text;

namespace Punfai.Report.Wpf
{
    public class WpfXamlReportType : IReportType
    {
        public WpfXamlReportType()
        {
            this.Filler = new XmlFiller();
        }

        public string Name { get { return "WpfXaml"; } }
        public string DocumentType { get { return "xaml"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new XmlTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            string flow =
@"<FlowDocument FontFamily=""Calibri"" FontSize=""14"">
    <FlowDocument.Resources>
        <Style TargetType=""{{x:Type Paragraph}}"">
            <Setter Property=""Margin"" Value=""0""/>
        </Style>
    </FlowDocument.Resources>
    <Paragraph>
        data['Title'] = 'Hello world'
    </Paragraph>
    <Paragraph FontSize=""18"" FontWeight=""Bold"">
        {{Title}}
    </Paragraph>
    <Paragraph>
        data['somelist'] = listofpeople
    </Paragraph>
    <Paragraph mesh-repeat=""person in somelist"">
        {{person.FirstName}} {{person.Surname}}
    </Paragraph>


    <Table CellSpacing=""0"" Margin=""0"">
        <Table.Columns>
            <TableColumn Width=""50""/>
            <TableColumn Width=""40""/>
            <TableColumn/>
        </Table.Columns>
        <TableRowGroup>
            <TableRow>
                <TableCell BorderBrush=""#444444"" BorderThickness=""0,0,0,1"">
                    <Paragraph>AAAA</Paragraph>
                </TableCell>
                <TableCell BorderBrush=""#444444"" BorderThickness=""0,0,0,1"">
                    <Paragraph>BBBB</Paragraph>
                </TableCell>
                <TableCell BorderBrush=""#444444"" BorderThickness=""0,0,0,1"">
                    <Paragraph>CCCC</Paragraph>
                </TableCell>
            </TableRow>
            <TableRow mesh-repeat=""person in somelist"">
                <TableCell TextAlignment=""Right"">
                    <Paragraph Margin=""1,0,3,0"">
                        {{person.Id}}
                    </Paragraph>
                </TableCell>
                <TableCell>
                    <Paragraph>
                        <TextBlock TextWrapping=""NoWrap"" TextTrimming=""CharacterEllipsis"">
                            {{person.FirstName}}
                        </TextBlock>
                    </Paragraph>
                </TableCell>
                <TableCell>
                    <Paragraph>
                        <TextBlock TextWrapping=""NoWrap"" TextTrimming=""CharacterEllipsis"">
                            {{person.Surname}}
                        </TextBlock>
                    </Paragraph>
                </TableCell>
            </TableRow>
        </TableRowGroup>
    </Table>
</FlowDocument>
";
            char[] chars = new char[flow.Length];
            flow.CopyTo(0, chars, 0, flow.Length);
            template = UTF8Encoding.UTF8.GetBytes(chars);
            return true;
        }
    }
}
