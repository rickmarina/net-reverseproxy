internal class CopyStreamException : Exception {

    public CopyStreamException()
    {
    }

    public CopyStreamException(string message) : base(message) {}

    public CopyStreamException(string message, Exception inner) : base(message, inner) {}

}