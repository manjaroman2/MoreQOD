namespace MoreQOD
{
    public class Feature
    {
        protected bool enabled;

        public Feature(bool enabled = true)
        {
            this.enabled = enabled;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}