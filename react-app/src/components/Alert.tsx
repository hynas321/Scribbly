interface AlertProps {
  title?: string;
  text: string;
  visible: boolean;
  type?: string;
}

function Alert({title, text, visible, type}: AlertProps) {
  return (
    <>
      {visible && (
        <div className={`alert alert-${ type == undefined ? "primary" : type }`} role="alert">
          { title && <h3>{title}</h3> }
          <h6>{text}</h6>
        </div>
      )}
    </>
  );
}

export default Alert;