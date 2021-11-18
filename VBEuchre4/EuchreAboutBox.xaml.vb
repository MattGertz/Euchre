Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Data
Imports System.Xml

Partial Public Class EuchreAboutBox
    ''' <summary>
    ''' Default constructor is protected so callers must use one with a parent.
    ''' </summary>
    Protected Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Constructor that takes a parent for this WpfAboutBoxVB1 dialog.
    ''' </summary>
    ''' <param name="parent">Parent window for this dialog.</param>
    Public Sub New(ByVal parent As Window)
        Me.New()
        Me.Owner = parent
    End Sub

    ''' <summary>
    ''' Handles click navigation on the hyperlink in the About dialog.
    ''' </summary>
    ''' <param name="sender">Object the sent the event.</param>
    ''' <param name="e">Navigation events arguments.</param>
    Private Sub hyperlink_RequestNavigate(ByVal sender As Object, ByVal e As System.Windows.Navigation.RequestNavigateEventArgs)
        If e.Uri IsNot Nothing AndAlso String.IsNullOrEmpty(e.Uri.OriginalString) = False Then
            Dim uri As String = e.Uri.AbsoluteUri
            Process.Start(New ProcessStartInfo(uri))
            e.Handled = True
        End If
    End Sub

#Region "AboutData Provider"
#Region "Member data"
    Private xmlDoc As XmlDocument = Nothing

    Private Const propertyNameTitle As String = "Title"
    Private Const propertyNameDescription As String = "Description"
    Private Const propertyNameProduct As String = "Product"
    Private Const propertyNameCopyright As String = "Copyright"
    Private Const propertyNameCompany As String = "Company"
    Private Const xPathRoot As String = "ApplicationInfo/"
    Private Const xPathTitle As String = xPathRoot + propertyNameTitle
    Private Const xPathVersion As String = xPathRoot & "Version"
    Private Const xPathDescription As String = xPathRoot + propertyNameDescription
    Private Const xPathProduct As String = xPathRoot + propertyNameProduct
    Private Const xPathCopyright As String = xPathRoot + propertyNameCopyright
    Private Const xPathCompany As String = xPathRoot + propertyNameCompany
    Private Const xPathLink As String = xPathRoot & "Link"
    Private Const xPathLinkUri As String = xPathRoot & "Link/@Uri"
#End Region

#Region "Properties"
    ''' <summary>
    ''' Gets the title property, which is display in the About dialogs window title.
    ''' </summary>
    Public ReadOnly Property ProductTitle() As String
        Get
            Dim result As String = CalculatePropertyValue(Of AssemblyTitleAttribute)(propertyNameTitle, xPathTitle)
            If String.IsNullOrEmpty(result) Then
                ' otherwise, just get the name of the assembly itself.
                result = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase)
            End If
            Return result
        End Get
    End Property

    ''' <summary>
    ''' Gets the application's version information to show.
    ''' </summary>
    Public ReadOnly Property Version() As String
        Get
            Dim result As String = String.Empty
            ' first, try to get the version string from the assembly.
            Dim ver As Version = Assembly.GetExecutingAssembly().GetName().Version
            If ver IsNot Nothing Then
                result = ver.ToString()
            Else
                ' if that fails, try to get the version from a resource in the Application.
                result = GetLogicalResourceString(xPathVersion)
            End If
            Return result
        End Get
    End Property

    ''' <summary>
    ''' Gets the description about the application.
    ''' </summary>
    Public ReadOnly Property Description() As String
        Get
            Return CalculatePropertyValue(Of AssemblyDescriptionAttribute)(propertyNameDescription, xPathDescription)
        End Get
    End Property

    ''' <summary>
    ''' Gets the product's full name.
    ''' </summary>
    Public ReadOnly Property Product() As String
        Get
            Return CalculatePropertyValue(Of AssemblyProductAttribute)(propertyNameProduct, xPathProduct)
        End Get
    End Property

    ''' <summary>
    ''' Gets the copyright information for the product.
    ''' </summary>
    Public ReadOnly Property Copyright() As String
        Get
            Return CalculatePropertyValue(Of AssemblyCopyrightAttribute)(propertyNameCopyright, xPathCopyright)
        End Get
    End Property

    ''' <summary>
    ''' Gets the product's company name.
    ''' </summary>
    Public ReadOnly Property Company() As String
        Get
            Return CalculatePropertyValue(Of AssemblyCompanyAttribute)(propertyNameCompany, xPathCompany)
        End Get
    End Property

    ''' <summary>
    ''' Gets the link text to display in the About dialog.
    ''' </summary>
    Public ReadOnly Property LinkText() As String
        Get
            Return GetLogicalResourceString(xPathLink)
        End Get
    End Property

    ''' <summary>
    ''' Gets the link uri that is the navigation target of the link.
    ''' </summary>
    Public ReadOnly Property LinkUri() As String
        Get
            Return GetLogicalResourceString(xPathLinkUri)
        End Get
    End Property
#End Region

#Region "Resource location methods"
    ''' <summary>
    ''' Gets the specified property value either from a specific attribute, or from a resource dictionary.
    ''' </summary>
    ''' <typeparam name="T">Attribute type that we're trying to retrieve.</typeparam>
    ''' <param name="propertyName">Property name to use on the attribute.</param>
    ''' <param name="xpathQuery">XPath to the element in the XML data resource.</param>
    ''' <returns>The resulting string to use for a property.
    ''' Returns null if no data could be retrieved.</returns>
    Private Function CalculatePropertyValue(Of T)(ByVal propertyName As String, ByVal xpathQuery As String) As String
        Dim result As String = String.Empty
        ' first, try to get the property value from an attribute.
        Dim attributes As Object() = Assembly.GetExecutingAssembly().GetCustomAttributes(GetType(T), False)
        If attributes.Length > 0 Then
            Dim attrib As T = DirectCast(attributes(0), T)
            Dim [property] As PropertyInfo = attrib.[GetType]().GetProperty(propertyName, BindingFlags.[Public] Or BindingFlags.Instance)
            If [property] IsNot Nothing Then
                result = TryCast([property].GetValue(attributes(0), Nothing), String)
            End If
        End If

        ' if the attribute wasn't found or it did not have a value, then look in an xml resource.
        If result = String.Empty Then
            ' if that fails, try to get it from a resource.
            result = GetLogicalResourceString(xpathQuery)
        End If
        Return result
    End Function

    ''' <summary>
    ''' Gets the XmlDataProvider's document from the resource dictionary.
    ''' </summary>
    Protected Overridable ReadOnly Property ResourceXmlDocument() As XmlDocument
        Get
            If xmlDoc Is Nothing Then
                ' if we haven't already found the resource XmlDocument, then try to find it.
                Dim provider As XmlDataProvider = TryCast(Me.TryFindResource("aboutProvider"), XmlDataProvider)
                If provider IsNot Nothing Then
                    ' save away the XmlDocument, so we don't have to get it multiple times.
                    xmlDoc = provider.Document
                End If
            End If
            Return xmlDoc
        End Get
    End Property

    ''' <summary>
    ''' Gets the specified data element from the XmlDataProvider in the resource dictionary.
    ''' </summary>
    ''' <param name="xpathQuery">An XPath query to the XML element to retrieve.</param>
    ''' <returns>The resulting string value for the specified XML element. 
    ''' Returns empty string if resource element couldn't be found.</returns>
    Protected Overridable Function GetLogicalResourceString(ByVal xpathQuery As String) As String
        Dim result As String = String.Empty
        ' get the About xml information from the resources.
        Dim doc As XmlDocument = Me.ResourceXmlDocument
        If doc IsNot Nothing Then
            ' if we found the XmlDocument, then look for the specified data. 
            Dim node As XmlNode = doc.SelectSingleNode(xpathQuery)
            If node IsNot Nothing Then
                If TypeOf node Is XmlAttribute Then
                    ' only an XmlAttribute has a Value set.
                    result = node.Value
                Else
                    ' otherwise, need to just return the inner text.
                    result = node.InnerText
                End If
            End If
        End If
        Return result
    End Function
#End Region
#End Region

    Private Sub EuchreAboutBox_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        EuchreTable.SetIcon(Me, My.Resources.Euchre)
    End Sub
End Class
