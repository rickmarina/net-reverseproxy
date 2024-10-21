
using Xunit;

public class HttpRawParserTest {

    [Fact]
    public void Test_Simple_Get() { 
        string rawRequest = TestDataHttp.basic_GET;

        var parser = new HttpRawParser(rawRequest); 

        Assert.Equal("/home",parser.Uri.Url);
        Assert.Equal("", parser.Body.Body);
    }

}