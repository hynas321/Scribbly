import githubLogo from './../assets/github-logo.png';

export default function Footer() {
  return (
    <footer className="d-flex flex-wrap justify-content-center align-items-center border-top border-bottom">
      <span className="mt-3 mb-3 text-muted">
        <img
          src={githubLogo}
          alt="Table loading"
          className="img-fluid"
          style={{height: "2.5em"}}
        />
        <a href="https://github.com/hynas321">https://github.com/hynas321</a>
      </span>
    </footer>
  )
}
